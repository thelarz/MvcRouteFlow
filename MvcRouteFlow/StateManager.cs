using System.Collections.Generic;
using System.Linq;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{
    public class StateManager
    {
        public static readonly List<State> States = new List<State>();

        public static State GetState(string id)
        {
            return States.FirstOrDefault(x => x.SessionCookie == id);
        }

        public static State CreateState<T>(string cookie)
        {
            if (GetState(cookie) != null)
            {
                RemoveState(cookie);
            }

            var newstate = new State(cookie, typeof(T).Name);
            States.Add(newstate);
            return newstate;
        }

        public static void SetCorrelationId(string sessionid, string name, object id)
        {
            var state = GetState(sessionid);
            if (state.CorrelationIds.ContainsKey(name.ToLower()))
            {
                state.CorrelationIds[name.ToLower()] = id;
            }
            else
            {
                state.CorrelationIds.Add(name.ToLower() ?? id.ToString(), id);
            }

        }

        public static object GetCorrelationId(string sessionid, string name)
        {
            var state = GetState(sessionid);
            if (state.CorrelationIds.ContainsKey(name.ToLower()))
            {
                var value = state.CorrelationIds[name.ToLower() ?? "id"];
                return value;
            }
            return null;
        }



        /*
        public void SyncronizeSteps(string id, int step)
        {

            var state = GetState(id);

            if (state == null)
            {
                throw new RouteFlowException("A RoutFlow session is not active, has timed-out or an attemp was made to click back through pages after the workflow has completed.");
            }

            // We know we're moving forward normally so get out.

            if (state.MovingForward)
                return;

            // Someone clicked a "link" to a new step (.Next()) was not the source, so if we're 
            // at the step we're expecting, just get out.

            if (step == state.Current.Step)
                return;

            // Syncronice Backward:

            if (step == state.Entries.ToArray()[1].Step)
            {

                state.Entries.Pop();

                state.Current = state.Entries.Peek();
                state.Current.Completed = false;
                state.Current.OnBeforeCompleted = false;
                return;

            }

            // Syncronize a Jump Forward:
            // This little chunk of code will handle the scenario where step = 3 (when.no) on the same method
            // with step = 10 (when.yes) and successfully make the jump from step 3 to 10 and still ask the before question.
            // could easily break other stuff, but hopefully conditions about will cause the routine to exit prior
            // to this code executing most of the time.

            state.Current = new StateEntry()
            {
                Step = step,
            };

        }
        */

        public static void RemoveState(string id)
        {
            var state = GetState(id);
            if (state != null)
            {
                States.Remove(state);
            }
        }

    }
}