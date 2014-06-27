using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcRouteFlow;
using NUnit.Framework;

namespace MvcRouteFlow.Tests
{

    

    [TestFixture()]
    public class WhenTestingTheStateManager
    {

        

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

        [SetUp]
        public void Setup()
        {
            test = new TestFlow();
            test.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            test.TearDown();
        }

        [Test()]
        public void New_States_Are_Created_Correctly()
        {
            var state = StateManager.CreateState<TestFlow>(Cookie);

            Assert.AreEqual(state.Path, typeof (TestFlow).Name);
        }

        [Test()]
        public void New_States_Are_Accessible()
        {
            StateManager.CreateState<TestFlow>(Cookie);
            var state = StateManager.GetState(Cookie);

            Assert.IsNotNull(state);
        }

        [Test()]
        public void State_Path_Is_Set_Correctly()
        {
            StateManager.CreateState<TestFlow>(Cookie);
            
            Assert.AreEqual("TestFlow", StateManager.GetState(Cookie).Path);
        }

        
        [Test()]
        public void States_Can_Be_Removed()
        {
            var state = StateManager.CreateState<TestFlow>(Cookie);
            StateManager.RemoveState(state.SessionCookie);
            state = StateManager.GetState("COOKIE");

            Assert.IsNull(state);
        }

        [Test()]
        public void Can_Set_And_Get_Correlation_Ids()
        {
            var state = StateManager.CreateState<TestFlow>(Cookie);

            Assert.IsNotNull(state);

            var testOne = StateManager.GetCorrelationId(Cookie, "id");

            Assert.IsNull(testOne);

            StateManager.SetCorrelationId(Cookie, "id", 9999);

            var testTwo = StateManager.GetCorrelationId(Cookie, "id");

            Assert.AreEqual(9999, (int)testTwo);
        }

    }

}
