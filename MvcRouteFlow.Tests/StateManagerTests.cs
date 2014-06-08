using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcRouteFlow;
using NUnit.Framework;

namespace MvcRouteFlow.Tests
{

    public class TestFlow : IHandleRouteFlowInitialization
    {

        public void Setup()
        {
            var flow = new Path<TestFlow>()

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
            throw new NotImplementedException();
        }
    }

    [TestFixture()]
    public class WhenTestingTheStateManager
    {
        private const string Cookie = "COOKIE";
        private StateManager sm;



        [SetUp]
        public void Setup()
        {
        }

        [Test()]
        public void NewStatesAreCreatedCorrectly()
        {

            var state = StateManager.CreateState<TestFlow>(Cookie);

            Assert.AreEqual(state.Path, typeof (TestFlow).Name);

        }

        [Test()]
        public void NewStatesAreAccessible()
        {
            const string path = "test-path";

            var state1 = StateManager.CreateState<TestFlow>(Cookie);
            var state2 = StateManager.CreateState<TestFlow>("COOKIE");

            Assert.AreEqual(state1.Path, state2.Path);

        }

        
        [Test()]
        public void StatesCanBeRemoved()
        {
            const string path = "test-removble-state";

            var state = StateManager.CreateState<TestFlow>(Cookie);
            
            // remove state here

            state = StateManager.GetState("COOKIE");

            Assert.IsNull(state);

        }

    }

}
