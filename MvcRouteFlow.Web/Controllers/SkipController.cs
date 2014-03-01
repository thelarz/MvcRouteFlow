using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcRouteFlow.Web.Controllers
{
    public class SkipController : Controller
    {
        //
        // GET: /Skip/

        public ActionResult Index()
        {
            return RouteFlow.Begin("path-bar");
        }

        [RouteFlow(Path = "path-bar", Step = 1, Select = When.Auto)]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page1(string post)
        {
            return RouteFlow.Next(new { id = 22 });
        }

        [RouteFlow(Path = "path-bar", Step = 2, Select = When.Auto)]
        [RouteFlow(Path = "path-bar", Step = 2, Select = When.No)]
        
        [RouteFlowBefore(Path = "path-bar", Step = 2, Message = "Thanks for completing step one", Question = "Would you like to skip step 2?")]
        public ActionResult Page2(int id)
        {
            return View();
        }


        [HttpPost]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next(new { id = 22 });
        }

        [RouteFlow(Path = "path-bar", Step = 3, Select = When.Auto)]
        [RouteFlow(Path = "path-bar", Step = 3, Select = When.Yes)]
        [RouteFlow(Path = "path-bar", Step = 2, Select = When.Yes)]
        [RouteFlowBefore(Path = "path-bar", Step = 3, Message = "Let's move on...", Question = "Would you like to DO step 3?")]
        [RouteFlowSync(Path = "path-bar", Step = 3)]
        public ActionResult Page3(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next();
        }

        [RouteFlow(Path = "path-bar", Step = 3, Select = When.No)]
        [RouteFlow(Path = "path-bar", Step = 4, Select = When.Auto)]
        [RouteFlowSync(Path = "path-bar", Step = 4)]
        public ActionResult Page4()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page4(string post)
        {
            return RouteFlow.Next();
            return View();
        }

        [RouteFlow(Path = "path-bar", Select = When.Done)]
        public ActionResult Done()
        {
            RouteFlow.Done();
            return View();
        }

    }
}
