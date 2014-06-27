using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcRouteFlow;
using MvcRouteFlow.Exceptions;
using NUnit.Framework;

namespace MvcRouteFlow.Tests
{

    

    [TestFixture()]
    public class WhenTestingThePathManager
    {

        public class NotInstalledFlow : IHandleRouteFlowInitialization
        {

            private IPath flow;

            public void Setup()
            {
                flow = new Path<NotInstalledFlow>()
                    .AddStep(new SimpleStep()
                    {
                        Name = "Im-not-installed",
                        Controller = "Test",
                        Action = "Page1"

                    })
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

        public class NoStepsFlow : IHandleRouteFlowInitialization
        {

            private IPath flow;

            public void Setup()
            {
                flow = new Path<NoStepsFlow>()
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
        public class InterstialFirstStepFlow : IHandleRouteFlowInitialization
        {

            private IPath flow;

            public void Setup()
            {
                flow = new Path<InterstialFirstStepFlow>()
                    .AddStep(new Interstitial()
                    {
                        Name = "Step1-Question",
                        Question = "Does this work?",
                        NoLabel = "No",
                        YesLabel = "Yes"
                    })
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

        public class TestFlow : IHandleRouteFlowInitialization
        {

            private IPath flow;

            public void Setup()
            {
                flow = new Path<TestFlow>()
                    .AddStep(new SimpleStep()
                    {
                        Name = "Test-Page1",
                        Controller = "Test",
                        Action = "Page1"

                    })
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

        private const string Cookie = "COOKIE";
        private StateManager sm;

        private TestFlow test;
        private InterstialFirstStepFlow interstialFirstStepFlow;

        [SetUp]
        public void Setup()
        {
            test = new TestFlow();
            test.Setup();

            interstialFirstStepFlow = new InterstialFirstStepFlow();
            interstialFirstStepFlow.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            test.TearDown();
            interstialFirstStepFlow.TearDown();
        }

       
        [Test]
        public void Route_Flows_Must_Be_Installed()
        {
            Assert.Throws<RouteFlowException>(() =>
                                                  {
                                                      var state =
                                                          PathManager.GetStartingEndpoint(
                                                              typeof (NotInstalledFlow).Name);
                                                  });
        }

        [Test]
        public void FLow_Must_Have_Steps()
        {
            var flow = new NoStepsFlow();
            Assert.Throws<RouteFlowException>(() => flow.Setup());
            flow.TearDown();
        }

        [Test]
        public void Starting_point_Is_Found()
        {
            var state = PathManager.GetStartingEndpoint(typeof(TestFlow).Name);
            Assert.AreEqual("Test-Page1", state.StepName);
        }

        [Test]
        public void Starting_point_Can_Be_An_Interstitial()
        {
            var state = PathManager.GetStartingEndpoint(typeof(InterstialFirstStepFlow).Name);
            Assert.IsInstanceOf<Interstitial>(state.Step);
        }

       

    }

}
