using System;
using System.Web;
using System.Web.Mvc;

namespace MvcRouteFlow
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

    public class RouteFlowController : Controller
    {

        [NoCache]
        public ActionResult Interstitial(string message, string question, string yesroute, string yeslabel, string noroute, string nolabel)
        {
            //RouteFlow.Sync(step);
            var model = new InterstitialViewModel()
                            {
                                Message = message,
                                Question = question,
                                YesRoute = yesroute,
                                YesLabel = yeslabel,
                                NoRoute = noroute,
                                NoLabel = nolabel
                            };
            return View("RouteFlow", model);
        }

        [HttpPost]
        public ActionResult NextNO(string skiptono)
        {
            return RouteFlow.SkipTo(skiptono);
        }

        [HttpPost]
        public ActionResult NextYES(string skiptoyes)
        {
            return RouteFlow.SkipTo(skiptoyes);
        }

        public ActionResult Resume()
        {
            return RouteFlow.Resume();
        }

    }

}