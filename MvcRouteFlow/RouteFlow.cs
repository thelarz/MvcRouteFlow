using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

    public class PathManager
    {
        // TODO: extract the path access from the RouteFlow class so it can be tested
    }

    public class RouteFlow
    {

        //TODO: Get rid of all the static crap.

        //TODO: Would like this to be injected by the consumer if desired.
        public static StateManager StateManager { get; set; }

        static readonly List<Path> Paths = new List<Path>();

        static RouteFlow()
        {
            StateManager = new StateManager();
        }


        public static ActionResult Begin(string path)
        {

            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] == null
            //           ? string.Empty
            //           : HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;



            //HttpContext.Current.Session["routeflow"] = path;

            var cookie = HttpContext.Current.Session.SessionID;

            StateManager.CreateState(new State()
                                            {
                                                SessionCookie = cookie,
                                                Path = path,
                                                Step = 1
                                            });

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

        public static ActionResult Next(Controller controller)
        {
            return Next(controller, null);
        }

        public static ActionResult Next(Controller controller, object ids)
        {

            var routeValues = GetRouteValueDictionary(ids);

            var cookie = HttpContext.Current.Session.SessionID;

            var state = StateManager.GetState(cookie);
            if (state == null)
            {
                throw new ApplicationException("SessionID not valid in RouteFlow table");
            }

            var e = GetAfter(state.Path, state.Step);
            if (e != null)
            {
                // "After" question exists, load the model with the question text
                dynamic model = new ExpandoObject();

                model.Question = e.Question;
                model.Message = e.Message;

                // Get the next step and load the model with the controller/actions for the different responses
                state.Step++;
                var steps = GetNextSteps(state.Path, state.Step);


                model.YesRoute = controller.Url.Action(steps.First(x => x.Select == When.Yes).Action,
                                                       steps.First(x => x.Select == When.Yes).Controller, routeValues);

                //model.YesController = steps.First(x => x.Select == When.Yes).Controller;
                //model.YesAction = steps.First(x => x.Select == When.Yes).Action;
                model.YesLabel = steps.First(x => x.Select == When.Yes).Label ?? "Yes";

                model.NoRoute = controller.Url.Action(steps.First(x => x.Select == When.No).Action,
                                                       steps.First(x => x.Select == When.No).Controller, routeValues);


                //model.NoController = steps.First(x => x.Select == When.No).Controller;
                //model.NoAction = steps.First(x => x.Select == When.No).Action;
                model.NoLabel = steps.First(x => x.Select == When.No).Label ?? "No";

                var view = new ViewResult()
                               {
                                   ViewName = "RouteFlow",
                                   ViewData = new ViewDataDictionary(model)
                               };

                view.ViewData.Model = model;

                return view;
            
            }

            // Normal step next, 
            var result = GetNextEndpoint(state.Path, state.Step);

            // This feels slightly wrong.
            state.Step++;

            if (result == null)
            {
                // stumped
                return null;
            }

            routeValues.Add("controller", result.Controller);
            routeValues.Add("action", result.Action);

            return new RedirectToRouteResult(routeValues);

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

        public static Endpoint GetAfter(string path, int step)
        {
            var curr =
                Paths.FirstOrDefault(x => x.Key == path)
                      .Steps.FirstOrDefault(s => s.Id == step);

            var endpoint = curr.Endpoints.FirstOrDefault(e => e.Select == When.After);
            return endpoint;

        }

        public static List<Endpoint> GetNextSteps(string path, int step)
        {
            var curr =
                Paths.FirstOrDefault(x => x.Key == path)
                      .Steps.FirstOrDefault(s => s.Id == step);

            var endpoints = curr.Endpoints.Where(e => e.Select == When.Yes || e.Select == When.No);
            return endpoints.ToList();

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

            //TODO: Extract to its own class and write some unit tests around this

            // http://stackoverflow.com/questions/15844380/scan-for-all-actions-in-the-site

            var controllers = Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Controller))).ToList();
            controllers.ForEach((x) => GetActions(x));
        }
        
        private static void GetActions(Type controller)
        {

            //TODO: Extract to its own class and write some unit tests around this

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
                                               Select = attr.Select,
                                               Message = attr.Message,
                                               Question = attr.Question,
                                               Controller = controllerDesc.ControllerName,
                                               Action = action.ActionName,
                                               Label = attr.Label
                                           });
                    }

                }

               
            }
        }

    }
   
}
