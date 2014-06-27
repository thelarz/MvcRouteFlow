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
            var cookie = id;
            return States.FirstOrDefault(x => x.SessionCookie == cookie);
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

        public static State CreateTransientState(string cookie)
        {
            if (GetState(cookie) != null)
            {
                RemoveState(cookie);
            }

            var newstate = new State(cookie, "transient");
            States.Add(newstate);


            return newstate;
        }

        public static void SetCorrelationId(string cookie, string name, object id)
        {
            var state = GetState(cookie);
            if (state.CorrelationIds.ContainsKey(name.ToLower()))
            {
                state.CorrelationIds[name.ToLower()] = id;
            }
            else
            {
                state.CorrelationIds.Add(name.ToLower() ?? id.ToString(), id);
            }

        }

        public static object GetCorrelationId(string cookie, string name)
        {
            var state = GetState(cookie);
            if (state.CorrelationIds.ContainsKey(name.ToLower()))
            {
                var value = state.CorrelationIds[name.ToLower() ?? "id"];
                return value;
            }
            return null;
        }

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