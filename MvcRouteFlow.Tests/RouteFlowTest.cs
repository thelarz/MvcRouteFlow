using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using MvcRouteFlow;
using NUnit.Framework;

namespace MvcRouteFlow.Tests
{

    public class TestSessionProvider : IProvideRouteFlowSession
    {

        private string _sessionid;
        public string SessionId
        {
            get
            {
                if (_sessionid == null)
                {
                    _sessionid = new Random(DateTime.Now.Millisecond).GetHashCode().ToString();
                    Trace.WriteLine("Session is now" + _sessionid);
                }
                return _sessionid;
            }
        }
    }
    

    [TestFixture()]
    public class When_Testing_RouteFlow
    {

        public class KillerFlow : IHandleRouteFlowInitialization
        {

            private IPath flow;

            public void Setup()
            {
                flow = new Path<KillerFlow>()
                    .AddStep(new SimpleStep()
                    {
                        Name = "killer-page1",
                        Controller = "Test",
                        Action = "Page1"
                    })
                    .AddStep(new SimpleStep()
                                 {
                                     Name = "killer-page2",
                                     Controller = "Test",
                                     Action = "Page2"
                                 }
                                 .After("killer-page1"))
                    .Install();
            }

            public void TearDown()
            {
                if (flow != null)
                {
                    flow.Remove();
                }
            }

        }

        private const string SessionId = "COOKIE";
        private StateManager sm;

        private KillerFlow flow;

        [SetUp]
        public void Setup()
        {
            // bootstrap routeflow for testing
            var worker = new RouteFlow(new TestSessionProvider());

            flow = new KillerFlow();
            flow.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            flow.TearDown();
        }

        [Test]
        public void Begin_Will_Return_The_First_Step()
        {
            var step = RouteFlow.Begin<KillerFlow>() as RedirectToRouteResult;
            Assert.IsTrue(step.RouteValues.ContainsKey("controller"));
            Assert.AreEqual("Test", step.RouteValues["controller"]);
            Assert.AreEqual("Page1", step.RouteValues["action"]);
        }

        [Test, Ignore]
        public void Move_To_Next_Step()
        {
            var step = RouteFlow.Begin<KillerFlow>() as RedirectToRouteResult;
            var next = RouteFlow.Next() as RedirectToRouteResult;

            Assert.IsTrue(next.RouteValues.ContainsKey("controller"));
            Assert.AreEqual("Test", next.RouteValues["controller"]);
            Assert.AreEqual("Page2", next.RouteValues["action"]);
        }

        [Test]
        public void SkipTo_Step()
        {
            var step = RouteFlow.Begin<KillerFlow>() as RedirectToRouteResult;

            var next = RouteFlow.SkipTo("killer-page2") as RedirectToRouteResult;

            Assert.IsTrue(next.RouteValues.ContainsKey("controller"));
            Assert.AreEqual("Test", next.RouteValues["controller"]);
            Assert.AreEqual("Page2", next.RouteValues["action"]);
        }

    }

}
