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
        [RouteFlowSync(Path = "path-bar", Step = 1)]
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
        [RouteFlowSync(Path = "path-bar", Step = 2)]
        public ActionResult Page2(int id)
        {
            return View();
        }


        [HttpPost]
        public ActionResult Page2(string skip)
        {
            if (skip.ToLower() == "skip")
            {
                return RouteFlow.Next(new { id = 22}, 1);
            }
            return RouteFlow.Next(new { id = 22 });
        }

        [RouteFlow(Path = "path-bar", Step = 3, Select = When.Auto)]
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
