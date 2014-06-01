using System;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI.WebControls;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{
    public class RouteFlow
    {

        //TODO: Get rid of all the static crap.

        //TODO: Would like this to be injected by the consumer if desired.
        public static StateManager StateManager { get; set; }

        static RouteFlow()
        {
            StateManager = new StateManager();
        }

        public static string Dump 
        { 
            get
            {
                var cookie = HttpContext.Current.Session.SessionID;
                var state = StateManager.GetState(cookie);
                return string.Format("Completed/{0}/Current/{1}/Of/{2}", state == null ? "-" : state.LastCompletedStep.ToString(), state == null ? "-" : state.Current.Step.ToString(),
                    state == null ? "-" : state.MaxSteps.ToString());
            }
        }

        public static bool Active(int? id)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            if (state == null)
                return false;
            if (StateManager.GetCorrelationId(cookie, "key") == null)
                return false;
            return (int)StateManager.GetCorrelationId(cookie, "key") == id;
        }

        public static bool Active()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            return StateManager.GetState(cookie) != null;
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

        public static void Sync(int step)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            StateManager.SyncronizeSteps(cookie, step);
        }

        public static bool AtStep(int step)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);

            if (state == null)
                return false;

            return state.Current.Step == step;
        }

        public static bool IsBeforeCompleted()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            return state.Current.OnBeforeCompleted;
        }

        public static void BeforeCompleted()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            state.Current.OnBeforeCompleted = true;
        }

        //public static bool HasVisited(int step)
        //{
        //    var cookie = HttpContext.Current.Session.SessionID;
        //    var state = StateManager.GetState(cookie);
        //    return state.StepCompleted >= step;
        //}

        
        public static ActionResult Begin(string path)
        {

            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] == null
            //           ? string.Empty
            //           : HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;


            HttpContext.Current.Session["routeflow"] = path;

            var cookie = HttpContext.Current.Session.SessionID;


            StateManager.CreateState(cookie, path);

            var result = PathManager.GetStartingEndpoint(path);

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action, area = "" }));

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

        public static ActionResult Resume()
        {

            var routeValues = new RouteValueDictionary();

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new RouteFlowException("SessionID not valid in RouteFlow table");
            }

            StateManager.RevertBeforeCompleted(cookie);
            var result = PathManager.GetEndpoint(state.Path, state.Current.Step);
            if (result == null)
            {
                // stumped
                return null;
            }


            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);
            routeValues.Add("area", "");

            return new RedirectToRouteResult(routeValues);
        }

        public static ActionResult Next()
        {
            return Next(null, 0);
        }

        public static ActionResult Next(int skip)
        {
            return Next(null, skip);
        }

        public static ActionResult Next(object ids)
        {
            return Next(ids, 0);
        }

        public static ActionResult Next(object ids, int skip)
        {

            var routeValues = GetRouteValueDictionary(ids);

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new RouteFlowException("SessionID not valid in RouteFlow table");
            }

            StateManager.CompleteStep(cookie);
            state.Next(skip);

            var result = PathManager.GetEndpoint(state.Path, state.Current.Step);
            if (result == null)
            {
                // stumped
                return null;
            }

            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);
            routeValues.Add("area", "");

            return new RedirectToRouteResult(routeValues);
        }

        //public static void Skip(int skips)
        //{
        //    var cookie = HttpContext.Current.Session.SessionID;
        //    var state = StateManager.GetState(cookie);
        //    StateManager.CompleteStep(cookie);
        //    state.SkipThenNext(skips);

        //}

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

        public static void Register()
        {
            //TODO: Extract to its own class and write some unit tests around this
            PathManager.Initialize(Assembly.GetCallingAssembly());

        }

        public static void CleanUpRequest()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            state.MovingForward = false;

        }

    }
   
}
