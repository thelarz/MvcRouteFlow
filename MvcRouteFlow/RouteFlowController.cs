using System.Web.Mvc;

namespace MvcRouteFlow
{
    public class RouteFlowController : Controller
    {
        public ActionResult Interstitial(string message, string question, string yesroute, string yeslabel, string noroute, string nolabel)
        {
            var model = new RouteFlowViewModel()
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
    }
}