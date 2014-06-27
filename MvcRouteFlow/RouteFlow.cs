using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{

    /*
     * Limitations:
     * 
     * Controller post must have the same route as the get so that the PathManager.GetNextEndpoint can find the controller
     * Note: Maybe need to capture the get method controller/action (actually thought this was happening)
     * 
     */

    public static class RouteValueDictionaryExtensions
    {
        public static RouteValueDictionary Extend(this RouteValueDictionary dict, object ids)
        {
            if (dict == null)
                return null;

            if (ids != null)
            {
                foreach (var prop in ids.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    dict.Add(prop.Name, prop.GetValue(ids, null));
                }
            }
    
            return dict;
        }
    }

    public class RouteFlow
    {

        public static IProvideRouteFlowSession _SessionProvider { get; set; }

        public static IProvideRouteFlowSession SessionProvider
        {
            get
            {
                if (_SessionProvider != null)
                {
                    return _SessionProvider;
                }
                return new HttpSessionProvider();
            }
        }

        public RouteFlow(IProvideRouteFlowSession provider)
        {
            _SessionProvider = provider;
        }

        public static ActionResult Begin<T>()
        {
            StateManager.CreateState<T>(SessionProvider.SessionId);

            var result = PathManager.GetStartingEndpoint(typeof(T).Name);

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action, area = "" }));
        }

        public static void Prepare(ActionExecutingContext context)
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return;

            state.Current.SetContext(context);
        }

        public static bool Active()
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            return state != null;
        }

        public static bool Active<T>()
        {
            var state = StateManager.GetState(SessionProvider.SessionId);

            return state != null && state.Path == typeof (T).Name;
        }

        public static bool Active(int? id)
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return false;

            var primaryKey = StateManager.GetCorrelationId(SessionProvider.SessionId, "key");

            if (primaryKey == null)
                return false;

            return (int)primaryKey == id;
        }

        public static bool Active<T>(int? id)
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return false;

            var primaryKey = StateManager.GetCorrelationId(SessionProvider.SessionId, "key");

            if (primaryKey == null)
                return false;
            
            return state.Path == typeof (T).Name && (int)primaryKey == id;
        }

        public static bool OnPath<T>()
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return false;

            return (state.Path == typeof(T).Name);
        }

        public static bool OnPath(string path)
        {
            if (path == null)
                return false;

            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return false;

            return (state.Path == path);
        }
        
        
        public static string Dump
        {
            get 
            { 
                var state = StateManager.GetState(SessionProvider.SessionId);
                return state.Path + state.Current.Name;
            }
        }

        public static ActionResult SkipTo(string name)
        {
            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
                return null;
            state.Current.SkipTo = name;
            return Next(null);
        }

        public static ActionResult Next()
        {
            return Next(null);
        }

        public static ActionResult Next(object ids)
        {

            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
            {
                throw new RouteFlowException("RouteFlow:SessionID not valid in RouteFlow table");
            }

            Endpoint result;

            if (string.IsNullOrEmpty(state.Current.SkipTo))
            {
                result = PathManager.GetNextEndpoint(state.Path, state.Current.Controller, state.Current.Action);
            }
            else
            {
                result = PathManager.GetEndpointByName(state.Path, state.Current.SkipTo);
            }

            if (result == null)
            {
                // stumped
                return null;
            }

            var routeValues = GetRouteValueDictionary(ids);

            if (result.Correlations != null)
            {
                result.Correlations.ForEach(x =>
                                                {
                                                    if (x.Type == "SET")
                                                    {
                                                        if (routeValues.ContainsKey(x.RouteItem))
                                                            SetCorrelationId(x.Key, routeValues[x.RouteItem]);
                                                    }
                                                    if (x.Type == "GET")
                                                    {
                                                        var value = GetCorrelationId(x.Key);
                                                        if (value == null && x.IsRequired)
                                                            throw new RouteFlowException(string.Format("RouteFlow Correlation ({0}) is Required but not present", x.Key));
                                                        if (routeValues.ContainsKey(x.RouteItem))
                                                            routeValues[x.RouteItem] = value;
                                                        else
                                                            routeValues.Add(x.RouteItem, value);
                                                    }
                                                });
            }


            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);
            routeValues.Add("area", result.Area);

            routeValues.Extend(result.RouteValues);

            state.Current.SkipTo = null;
            state.Current.Name = result.StepName;

            return new RedirectToRouteResult(routeValues); 
        }

        public static ActionResult Resume()
        {

            var state = StateManager.GetState(SessionProvider.SessionId);
            if (state == null)
            {
                throw new RouteFlowException("RouteFlow:SessionID not valid in RouteFlow table");
            }

            var result = PathManager.GetEndpointByName(state.Path, state.Current.Name);
            if (result == null)
            {
                // stumped
                return null;
            }

            var routeValues = new RouteValueDictionary();

            if (result.Correlations != null)
            {
                result.Correlations.ForEach(x =>
                                                {
                                                    if (x.Type == "SET")
                                                        if (routeValues.ContainsKey(x.RouteItem))
                                                            SetCorrelationId(x.Key, routeValues[x.RouteItem]);
                                                            //SetCorrelationId(x.Key, state.Current.Context.ActionParameters[x.RouteItem]);
                                                    if (x.Type == "GET")
                                                    {
                                                        var value = GetCorrelationId(x.Key);
                                                        if (value == null && x.IsRequired)
                                                            throw new ApplicationException(string.Format("RouteFlow:Correlation ({0}) is Required but not present", x.Key));
                                                        if (routeValues.ContainsKey(x.RouteItem))
                                                            routeValues[x.RouteItem] = value;
                                                        else
                                                            routeValues.Add(x.RouteItem, value);
                                                    }
                                                });
            }


            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);
            routeValues.Add("area", result.Area);

            routeValues.Extend(result.RouteValues);

            state.Current.SkipTo = null;
            state.Current.Name = result.StepName;

            return new RedirectToRouteResult(routeValues); 
        }

        public static void Done()
        {
            StateManager.RemoveState(SessionProvider.SessionId);
        }

        private static RouteValueDictionary GetRouteValueDictionary(object ids)
        {
            var routeValues = new RouteValueDictionary();
            if (ids != null)
            {
                foreach (var prop in ids.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    routeValues.Add(prop.Name, prop.GetValue(ids, null));
                }
            }

            return routeValues;

        }

        public static void SetCorrelationId(string name, object id)
        {

            var state = StateManager.GetState(SessionProvider.SessionId);

            if (state == null)
            {
                throw new RouteFlowException("RouteFlow:SetCorrelationId:Session state invalid");
            }

            if (name == "key")
            {
                // Verify duplicate correlation key while in a session
                // Reserved "key" is used to correlate an entire workflow to a single object id
                var keyValue = GetCorrelationId("key");
                if (keyValue != null)
                {
                    if (keyValue.ToString() != id.ToString())
                    {
                        throw new RouteFlowException("RouteFlow:SetCorrelationId:Cannot reassign the primary correlation key.");
                    }
                }
            }

            StateManager.SetCorrelationId(SessionProvider.SessionId, name, id);

        }

        public static object GetCorrelationId(string name)
        {
            return StateManager.GetCorrelationId(SessionProvider.SessionId, name);
        }

        public static object GetCorrelationId<T>(string name)
        {
            return OnPath<T>() ? StateManager.GetCorrelationId(SessionProvider.SessionId, name) : null;
        }


        public static void Register()
        {
            Initialize(Assembly.GetCallingAssembly());
        }

        public static void Initialize(Assembly assembly)
        {
            // Run Setup on each class implementing IHandleRouteFlowInitialization
            var handlers = assembly.GetTypes().Where(type => type.GetInterfaces()
                .Contains(typeof(IHandleRouteFlowInitialization)))
                .Select(x => Activator.CreateInstance(x) as IHandleRouteFlowInitialization).ToList();
            handlers.ForEach(x => x.Setup());
        }



    }

    


}
