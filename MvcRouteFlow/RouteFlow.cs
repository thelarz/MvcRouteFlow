using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace MvcRouteFlow
{

    public class RouteFlow
    {

        static readonly List<State> States = new List<State>(); 
        static readonly List<Path> Paths = new List<Path>();

        public static ActionResult Begin(string path)
        {

            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] == null
            //           ? string.Empty
            //           : HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;



            //HttpContext.Current.Session["routeflow"] = path;

            var cookie = HttpContext.Current.Session.SessionID;

            var state = States.FirstOrDefault(x => x.SessionCookie == cookie);
            if (state == null)
            {
                state = new State()
                {
                    SessionCookie = cookie,
                    Path = path,
                    Step = 1
                };
                States.Add(state);
            }

            var result = GetStartingEndpoint(path);

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action }));

        }

        public static Endpoint GetStartingEndpoint(string path)
        {
            var endpoint =
                Paths.FirstOrDefault(x => x.Key == path)
                      .Steps.FirstOrDefault(s => s.Id == 1)
                      .Endpoints.FirstOrDefault(e => e.Select == When.Auto);
            return endpoint;
        }

        public static ActionResult Next()
        {
            var cookie = HttpContext.Current.Session.SessionID;

            var state = States.FirstOrDefault(x => x.SessionCookie == cookie);
            if (state == null)
            {
                throw new ApplicationException("SessionID not valid in RouteFlow table");
            }

            var result = GetNextEndpoint(state.Path, state.Step);

            if (result == null)
            {
                return null;
            }

            state.Step++;

            return new RedirectToRouteResult(new RouteValueDictionary(new { controller = result.Controller, action = result.Action }));

        }

        public static Endpoint GetNextEndpoint(string path, int step)
        {
            var next =
                Paths.FirstOrDefault(x => x.Key == path)
                      .Steps.FirstOrDefault(s => s.Id == step + 1);

            if (next == null)
            {
                var done = Paths.FirstOrDefault(x => x.Key == path)
                      .Steps.FirstOrDefault(s => s.Endpoints.Any(x => x.Select == When.Done));

                if (done == null)
                    throw new ApplicationException(
                        "RouteFlow: you requested next, there was no next route and no When.Done selection");

                return done.Endpoints.First(e => e.Select == When.Done);

            }

            return next.Endpoints.FirstOrDefault(e => e.Select == When.Auto);

        }

        public static void Register()
        {
            var controllers = Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Controller))).ToList();
            controllers.ForEach((x) => GetActions(x));
        }
        
        private static void GetActions(Type controller)
        {

            // Get a descriptor of this controller
            var controllerDesc = new ReflectedControllerDescriptor(controller);

            // Look at each action in the controller
            foreach (var action in controllerDesc.GetCanonicalActions())
            {
                // Get any attributes (filters) on the action
                var attributes = action.GetCustomAttributes(typeof(RouteFlowAttribute), false).Where(filter => filter is RouteFlowAttribute);
                
                foreach (var a in attributes)
                {

                    var attr = a as RouteFlowAttribute;

                    if (attr == null)
                        continue;
                    
                    var path = Paths.FirstOrDefault(x => x.Key == attr.Path);
                    if (path == null)
                    {
                        path = new Path()
                                   {
                                       Key = attr.Path,
                                       Steps = new List<Step>()
                                   };
                        Paths.Add(path);
                    }

                    var step = path.Steps.FirstOrDefault(x => x.Id == attr.Step);
                    if (step == null)
                    {
                        step = new Step()
                                       {
                                           Id = attr.Step,
                                           Endpoints = new List<Endpoint>()
                                       };
                        path.Steps.Add(step);
                    }

                    var item = step.Endpoints.FirstOrDefault(x => x.Select == attr.Select);

                    if (item == null)
                    {
                        step.Endpoints.Add(new Endpoint()
                                           {
                                               Controller = controllerDesc.ControllerName,
                                               Action = action.ActionName,
                                               Select = attr.Select
                                           });
                    }

                }

               
            }
        }

    }
   
}
