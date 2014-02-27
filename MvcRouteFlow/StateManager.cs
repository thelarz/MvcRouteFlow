using System.Collections.Generic;
using System.Linq;

namespace MvcRouteFlow
{
    public class StateManager
    {

        static readonly List<State> States = new List<State>();

        public State GetState(string id)
        {
            return States.FirstOrDefault(x => x.SessionCookie == id);
        }

        public State CreateState(State state)
        {
            if (GetState(state.SessionCookie) != null)
            {
                RemoveState(state.SessionCookie);
            }
            States.Add(state);
            return state;
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