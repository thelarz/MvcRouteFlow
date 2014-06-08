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
     * Controller post must have the same route as the get so that the PathManage.GetNextEndpoint can find the controller
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

        public static ActionResult Begin<T>()
        {

            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] == null
            //           ? string.Empty
            //           : HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;


            HttpContext.Current.Session["routeflow"] = typeof(T).Name;

            var cookie = HttpContext.Current.Session.SessionID;


            StateManager.CreateState<T>(cookie);

            var result = PathManager.GetStartingEndpoint(typeof(T).Name);

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action, area = "" }));

        }

        public static void Prepare(ActionExecutingContext context)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return;

            state.Current.Context = context;
        }

        public static bool Active()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            return state != null;
        }

        public static bool Active<T>()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);

            return state != null && state.Path == typeof (T).Name;
        }

        public static bool Active(int? id)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return false;

            var primaryKey = StateManager.GetCorrelationId(cookie, "key");

            if (primaryKey == null)
                return false;

            return (int)primaryKey == id;
        }

        public static bool Active<T>(int? id)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return false;

            var primaryKey = StateManager.GetCorrelationId(cookie, "key");

            if (primaryKey == null)
                return false;
            
            return state.Path == typeof (T).Name && (int)primaryKey == id;
        }

        public static bool OnPath<T>()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return false;

            return (state.Path == typeof(T).Name);
        }

        public static bool OnPath(string path)
        {
            if (path == null)
                return false;

            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return false;

            return (state.Path == path);
        }

        //public static void Sync(int step)
        //{
        //    var cookie = HttpContext.Current.Session.SessionID;
        //    var state = StateManager.GetState(cookie);
        //    StateManager.SyncronizeSteps(cookie, step);
        //}

        
        public static ActionResult SkipTo(string name)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
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

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new ApplicationException("ouch, no sessionid"); // new RouteFlowException("SessionID not valid in RouteFlow table");
            }

            Endpoint result;

            if (string.IsNullOrEmpty(state.Current.SkipTo))
            {
                result = PathManager.GetNextEndpoint(state.Path, state.Current.Step, state.Current.Controller, state.Current.Action);
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
                                                        if (routeValues.ContainsKey(x.RouteItem))
                                                            SetCorrelationId(x.Key, routeValues[x.RouteItem]);
                                                            //SetCorrelationId(x.Key, state.Current.Context.ActionParameters[x.RouteItem]);
                                                    if (x.Type == "GET")
                                                    {
                                                        var value = GetCorrelationId(x.Key);
                                                        if (value == null && x.IsRequired)
                                                            throw new ApplicationException(string.Format("RouteFlow Correlation ({0}) is Required but not present", x.Key));
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

            state.Next();

            state.Current.SkipTo = null;
            state.Current.Name = result.StepName;

            return new RedirectToRouteResult(routeValues); 
        }

        public static ActionResult Resume()
        {

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new RouteFlowException("SessionID not valid in RouteFlow table");
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
                                                            throw new ApplicationException(string.Format("RouteFlow Correlation ({0}) is Required but not present", x.Key));
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

            state.Next();

            state.Current.SkipTo = null;
            state.Current.Name = result.StepName;

            return new RedirectToRouteResult(routeValues); 
        }

        public static void Done()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            StateManager.RemoveState(cookie);
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

            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);

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

            StateManager.SetCorrelationId(cookie, name, id);


        }

        public static object GetCorrelationId(string name)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            return StateManager.GetCorrelationId(cookie, name);
        }

        

        public static void Register()
        {
            //TODO: Extract to its own class and write some unit tests around this
            Initialize(Assembly.GetCallingAssembly());

        }

        public static void Initialize(Assembly assembly)
        {
            // http://stackoverflow.com/questions/15844380/scan-for-all-actions-in-the-site


            var handlers = assembly.GetTypes().Where(type => type.GetInterfaces()
                .Contains(typeof(IHandleRouteFlowInitialization)))
                .Select(x => Activator.CreateInstance(x) as IHandleRouteFlowInitialization).ToList();
            handlers.ForEach(x => x.Setup());

        }

    }

    


}
