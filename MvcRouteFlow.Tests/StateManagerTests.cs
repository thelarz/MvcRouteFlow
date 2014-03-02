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
        private const string Cookie = "COOKIE";
        private StateManager sm;

        [SetUp]
        public void Setup()
        {
            sm = new StateManager();
        }

        [Test()]
        public void NewStatesAreCreatedCorrectly()
        {
            const string path = "test-path";

            var state = sm.CreateState(Cookie, path);

            Assert.AreEqual(state.Path, path);

        }

        [Test()]
        public void NewStatesAreAccessible()
        {
            const string path = "test-path";

            sm.CreateState(Cookie, path);

            var state = sm.GetState("COOKIE");

            Assert.AreEqual(state.Path, path);

        }

        [Test()]
        public void StatesCanBeReused()
        {
            const string path = "test-reusable-state";
            const string newpath = "new-path";

            sm.CreateState(Cookie, path);

            var newstate = sm.CreateState(Cookie, newpath);

            Assert.AreEqual(newstate.Path, newpath);

        }

        [Test()]
        public void StatesCanBeRemoved()
        {
            const string path = "test-removble-state";

            sm.CreateState(Cookie, path);

            sm.RemoveState("COOKIE");

            var state = sm.GetState("COOKIE");

            Assert.IsNull(state);

        }

    }

}
