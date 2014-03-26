using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcRouteFlow.Web.Controllers
{

    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(System.Web.Mvc.ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            filterContext.HttpContext.Response.Expires = -1;
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoServerCaching();
            //filterContext.HttpContext.Response.Cache.SetAllowResponseInBrowserHistory(false);
            //filterContext.HttpContext.Response.CacheControl = "no-cache";
            filterContext.HttpContext.Response.Cache.SetNoStore();

        }
    }

    public class TestController : Controller
    {
        
        public ActionResult Index()
        {
            return RouteFlow.Begin("path-foo");
        }

        [NoCache]
        [RouteFlow(Path = "path-foo", Step = 1, Select = When.Auto)]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost, NoCache]
        [RouteFlowSetCorrelation(Path = "path-foo", As = "id", Value = "id")]
        public ActionResult Page1(string id)
        {
            return RouteFlow.Next();
        }

        [NoCache]
        [RouteFlow(Path = "path-foo", Step = 2, Select = When.Auto)]
        [RouteFlowBefore(Path = "path-foo", Step = 2, Message = "Thanks for completing step one", Question = "Please choose OptionA or OptionB")]
        [RouteFlow(Path = "path-foo", Step = 2, Select = When.Yes, Label = "OptionA")]
        public ActionResult Page2()
        {
            return View();
        }

        [NoCache]
        [RouteFlow(Path = "path-foo", Step = 2, Select = When.No, Label = "OptionB")]
        public ActionResult Page2B()
        {
            return View();
        }

        [HttpPost, NoCache]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next();
        }

        [NoCache]
        [RouteFlow(Path = "path-foo", Step = 3, Select = When.Auto)]
        public ActionResult Page3()
        {
            return View();
        }

        [HttpPost, NoCache]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next();
        }

        [NoCache]
        [RouteFlow(Path = "path-foo", Step = 4, Select = When.Done)]
        [RouteFlowGetCorrelation(Path = "path-foo", Name = "id", AssignTo = "id")]
        public ActionResult Done(object id)
        {
            RouteFlow.Done();
            return View(id);
        }

    }

}
