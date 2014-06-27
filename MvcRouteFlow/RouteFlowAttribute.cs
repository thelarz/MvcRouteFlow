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
   
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RouteFlowAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RouteFlow.Prepare(filterContext);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteFlowSetCorrelation : ActionFilterAttribute
    {
        public Type Path { get; set; }
        public string As { get; set; }
        public string Value { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!RouteFlow.OnPath(Path.Name))
                return;

            var actionValues = filterContext.ActionParameters;
            if (actionValues[Value] == null)
            {
                // Attempting to set a correlation with a null value (maybe due to restarting a routeflow step)
                var value = RouteFlow.GetCorrelationId(As);
                if (value != null)
                {
                    actionValues[Value] = RouteFlow.GetCorrelationId(As);
                    return;
                }
            }
            RouteFlow.SetCorrelationId(As, actionValues[Value]);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteFlowGetCorrelation : ActionFilterAttribute
    {
        public Type Path { get; set; }
        public string Name { get; set; }
        public string AssignTo { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!RouteFlow.OnPath(Path.Name))
                return;

            var actionValues = filterContext.ActionParameters;
            if (actionValues.ContainsKey(AssignTo))
                actionValues[AssignTo] = RouteFlow.GetCorrelationId(Name);
        }
    }

    
}