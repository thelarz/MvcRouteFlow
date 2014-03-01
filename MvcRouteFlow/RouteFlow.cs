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
                return string.Format("CurrrentStep: {0}", state == null ? "Empty" : state.Step.ToString());
            }
        }

        public static bool Active()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            return StateManager.GetState(cookie) != null;
        }

        public static bool OnPath(string path)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            return (state.Path == path);
        }

        public static void Sync(int step)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            StateManager.SyncronizeSteps(cookie, step);
        }

        public static bool IsBeforeCompleted()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            return state.StepOnBeforeCompleted == state.Step;
        }

        public static void BeforeCompleted()
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            state.StepOnBeforeCompleted = state.Step;
        }

        public static bool HasVisited(int step)
        {
            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);
            return state.StepCompleted >= step;
        }

        public static ActionResult Begin(string path)
        {

            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] == null
            //           ? string.Empty
            //           : HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;


            HttpContext.Current.Session["routeflow"] = path;

            var cookie = HttpContext.Current.Session.SessionID;
            

            StateManager.CreateState(new State()
                                            {
                                                SessionCookie = cookie,
                                                Path = path,
                                                Step = 1
                                            });

            var result = PathManager.GetStartingEndpoint(path);

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action }));

        }
        
        public static ActionResult Next()
        {
            return Next(null);
        }

        public static ActionResult Next(object ids)
        {

            var routeValues = GetRouteValueDictionary(ids);

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new ApplicationException("SessionID not valid in RouteFlow table");
            }

            StateManager.CompleteStep(cookie);
            state.Step++;

            var result = PathManager.GetEndpoint(state.Path, state.Step);
            if (result == null)
            {
                // stumped
                return null;
            }

            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);

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

        public static void Register()
        {
            //TODO: Extract to its own class and write some unit tests around this
            PathManager.Initialize(Assembly.GetCallingAssembly());

        }

    }
   
}
