using System;
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
        public ActionResult Page1(string post)
        {
            return RouteFlow.Next(new { id = 22 });
        }

        [RouteFlow(Path = "path-foo", Step = 2, Select = When.Auto)]
        [RouteFlowBefore(Path = "path-foo", Step = 2, Message = "Thanks for completing step one", Question = "Please choose OptionA or OptionB")]
        [RouteFlow(Path = "path-foo", Step = 2, Select = When.Yes, Label = "OptionA")]
        public ActionResult Page2()
        {
            return View();
        }

        [RouteFlow(Path = "path-foo", Step = 2, Select = When.No, Label = "OptionB")]
        public ActionResult Page2B()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next(new { id = 22 });
        }

        [RouteFlow(Path = "path-foo", Step = 3, Select = When.Auto)]
        public ActionResult Page3(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next();
        }

        [RouteFlow(Path = "path-foo", Select = When.Done)]
        public ActionResult Done()
        {
            RouteFlow.Done();
            return View();
        }

    }

}
