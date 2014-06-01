using System.Collections.Generic;
using System.Linq;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{
    public class StateManager
    {

        static readonly List<State> States = new List<State>();

        

        public State GetState(string id)
        {
            return States.FirstOrDefault(x => x.SessionCookie == id);
        }

        public State CreateState(string cookie, string path)
        {
            if (GetState(cookie) != null)
            {
                RemoveState(cookie);
            }

            var newstate = new State(cookie, path);
            States.Add(newstate);
            return newstate;
        }

        public void CompleteStep(string id)
        {
            var state = GetState(id);
            if (state != null)
            {
                state.Current.Completed = true;
            }
        }

        public void RevertBeforeCompleted(string id)
        {
            // really need a pop stack operation for moving backwards
            var state = GetState(id);
            if (state != null)
            {
                if (state.Current.OnBeforeCompleted)
                {
                    state.Current.OnBeforeCompleted = false;
                }
            }
        }

        public void SetCorrelationId(string sessionid, string name, object id)
        {
            var state = GetState(sessionid);
            if (state.CorrelationIds.ContainsKey(name))
            {
                state.CorrelationIds[name] = id;
            }
            else
            {
                state.CorrelationIds.Add(name ?? id.ToString(), id);    
            }
            
        }

        public object GetCorrelationId(string sessionid, string name)
        {
            var state = GetState(sessionid);
            if (state.CorrelationIds.ContainsKey(name))
                return state.CorrelationIds[name ?? "id"];
            return null;
        }

        public void SyncronizeSteps(string id, int step)
        {

            var state = GetState(id);

            if (state == null)
            {
                throw new RouteFlowException("A RoutFlow session is not active, has timed-out or an attemp was made to click back through pages after the workflow has completed.");
            }

            if (state.MovingForward)
                return;

            if (step == state.Current.Step)
                return;

            // ugh are we trying to back up?
            if (step == state.Entries.ToArray()[1].Step)
            {
                
                state.Entries.Pop();

                state.Current = state.Entries.Peek();
                state.Current.Completed = false;
                state.Current.OnBeforeCompleted = false;
            }
            //state.Step = step;
        }

        public void RemoveState(string id)
        {
            var state = GetState(id);
            if (state != null)
            {
                States.Remove(state);
            }
        }

    }
}