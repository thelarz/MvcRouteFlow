using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcRouteFlow.Web.Controllers
{
    public class EasyController : Controller
    {
        //
        // GET: /Easy/

        public ActionResult Index()
        {
            return RouteFlow.Begin("easy");
        }

        [HttpGet]
        [RouteFlow(Path = "easy", Step = 1, Select = When.Auto)]
        public ActionResult One()
        {
            return View();
        }

        [HttpPost]
        public ActionResult One(int? id)
        {
            return RouteFlow.Next();
        }

        [HttpGet]
        [RouteFlow(Path = "easy", Step = 2, Select = When.Auto)]
        public ActionResult Two()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Two(int? id)
        {
            return RouteFlow.Next();
        }

        [HttpGet]
        [RouteFlow(Path = "easy", Step = 3, Select = When.Auto)]
        public ActionResult Three()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Three(int? id)
        {
            return RouteFlow.Next();
        }

        [HttpGet]
        [RouteFlow(Path = "easy", Step = 200, Select = When.Done)]
        public ActionResult Done()
        {
            return View();
        }

        [HttpPost]
        [RouteFlow(Path = "easy", Step = 200, Select = When.Done)]
        public ActionResult Done(int? id)
        {
            return RedirectToAction("Index");
        }


    }
}
