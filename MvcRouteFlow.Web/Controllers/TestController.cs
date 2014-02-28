﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcRouteFlow.Web.Controllers
{

    public class TestController : Controller
    {
        
        public ActionResult Index()
        {
            return RouteFlow.Begin("path-foo");
        }

        [RouteFlow(Path = "path-foo", Step = 1, Select = When.Auto)]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost]
        [RouteFlow(Path = "path-foo", Step = 1, Select = When.After, 
            Message = "Thanks for completing step one", Question = "Would you like to move on?")]
        public ActionResult Page1(string post)
        {
            return RouteFlow.Next(this, new { id = 22 });
        }

        [RouteFlow(Path = "path-foo", Step = 2, Select = When.Yes)]
        public ActionResult Page2()
        {
            return View();
        }

        [RouteFlow(Path = "path-foo", Step = 2, Select = When.No, Label = "No thanks")]
        public ActionResult Page2No()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next(this, new { id = 22 });
        }

        [RouteFlow(Path = "path-foo", Step = 3, Select = When.Auto)]
        public ActionResult Page3(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next(this);
        }

        [RouteFlow(Path = "path-foo", Select = When.Done)]
        public ActionResult Done()
        {
            return View();
        }

    }

}
