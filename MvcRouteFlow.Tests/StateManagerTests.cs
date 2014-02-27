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

            var state = sm.CreateState(new State()
            {
                Path = path,
                Step = 1,
                SessionCookie = "COOKIE"
            });

            Assert.AreEqual(state.Path, path);

        }

        [Test()]
        public void NewStatesAreAccessible()
        {
            const string path = "test-path";

            sm.CreateState(new State()
                               {
                                   Path = path,
                                   Step = 1,
                                   SessionCookie = "COOKIE"
                               });

            var state = sm.GetState("COOKIE");

            Assert.AreEqual(state.Path, path);

        }

        [Test()]
        public void StatesCanBeReused()
        {
            const string path = "test-reusable-state";
            const string newpath = "new-path";

            sm.CreateState(new State()
            {
                Path = path,
                Step = 5,
                SessionCookie = "COOKIE"
            });

            var newstate = sm.CreateState(new State()
            {
                Path = newpath,
                Step = 1,
                SessionCookie = "COOKIE"
            });

            Assert.AreEqual(newstate.Path, newpath);

        }

        [Test()]
        public void StatesCanBeRemoved()
        {
            const string path = "test-removble-state";

            sm.CreateState(new State()
            {
                Path = path,
                Step = 5,
                SessionCookie = "COOKIE"
            });

            sm.RemoveState("COOKIE");

            var state = sm.GetState("COOKIE");

            Assert.IsNull(state);

        }

    }

}
