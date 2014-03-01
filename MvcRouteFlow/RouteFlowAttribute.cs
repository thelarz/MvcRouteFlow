using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcRouteFlow
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteFlowAttribute : Attribute
    {
        public string Path { get; set; }
        public int Step { get; set; }
        public int GoTo { get; set; }
        public When Select { get; set; }
        public string Message { get; set; }
        public string Question { get; set; }
        public string Label { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteFlowSync : ActionFilterAttribute
    {

        public string Controller { get; set; }
        public string Action { get; set; }
        public int Step { get; set; }
        public string Path { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!RouteFlow.OnPath(Path))
                return;

            RouteFlow.Sync(Step);

        }



        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }


    }

    public class RouteFlowViewModel
    {
        public string Question { get; set; }
        public string Message { get; set; }
        public string YesRoute { get; set; }
        public string YesLabel { get; set; }
        public string NoRoute { get; set; }
        public string NoLabel { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RouteFlowSetCorrelation : ActionFilterAttribute
    {
        public string Path { get; set; }
        public string As { get; set; }
        public string Value { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!RouteFlow.OnPath(Path))
                return;

            var actionValues = filterContext.ActionParameters;
            RouteFlow.SetCorrelationId(As, actionValues[Value]);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RouteFlowGetCorrelation : ActionFilterAttribute
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string AssignTo { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!RouteFlow.OnPath(Path))
                return;

            var actionValues = filterContext.ActionParameters;
            if (actionValues.ContainsKey(AssignTo))
                actionValues[AssignTo] = RouteFlow.GetCorrelationId(Name);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteFlowBefore : ActionFilterAttribute
    {

        public string Controller { get; set; }
        public string Action { get; set; }
        public int Step { get; set; }
        public string Path { get; set; }
        public string Message { get; set; }
        public string Question { get; set; }
        
        static StateManager StateManager = new StateManager();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (!RouteFlow.OnPath(Path))
                return;

            RouteFlow.Sync(Step);

            if (RouteFlow.IsBeforeCompleted())
                return;

            

            var routeValues = filterContext.RouteData.Values;

            var cookie = HttpContext.Current.Session.SessionID;
            var state = StateManager.GetState(cookie);

            dynamic model = new RouteFlowViewModel();

            model.Question = this.Question;
            model.Message = this.Message;

            // Get the next step and load the model with the controller/actions for the different responses
            var endpoints = PathManager.GetYesNoEndpointsForStep(state.Path, state.Step);

            var yesEndpoint = endpoints.FirstOrDefault(x => x.Select == When.Yes);

            if (yesEndpoint == null)
            {
                throw new ApplicationException(string.Format("RouteFlow: Cannot find When.Yes route for Question on Path [{0}] Step [{1}]", Path, Step));
            }
            //if (yesEndpoint.GoTo > 0)
            //    yesEndpoint = PathManager.GetEndpoint(state.Path, yesEndpoint.GoTo);

            model.YesRoute = new UrlHelper(filterContext.RequestContext).Action(yesEndpoint.Action, yesEndpoint.Controller, routeValues);
            model.YesLabel = endpoints.First(x => x.Select == When.Yes).Label ?? "Yes";

            var noEndpoint = endpoints.FirstOrDefault(x => x.Select == When.No);

            if (noEndpoint == null)
            {
                throw new ApplicationException(string.Format("RouteFlow: Cannot find When.No route for Question on Path [{0}] Step [{1}]", Path, Step));
            }

            //if (noEndpoint.GoTo > 0)
            //    noEndpoint = PathManager.GetEndpoint(state.Path, noEndpoint.GoTo);

            model.NoRoute = new UrlHelper(filterContext.RequestContext).Action(noEndpoint.Action, noEndpoint.Controller, routeValues);
            model.NoLabel = endpoints.First(x => x.Select == When.No).Label ?? "No";

            filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary 
                    {
                        { "action", "Interstitial" },
                        { "controller", "RouteFlow" },
                        { "Message", model.Message },
                        { "Question", model.Question },
                        { "YesRoute", model.YesRoute },
                        { "YesLabel", model.YesLabel },
                        { "NoRoute", model.NoRoute },
                        { "NoLabel", model.NoLabel }
                    });

            RouteFlow.BeforeCompleted();
            
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

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }


    }

}