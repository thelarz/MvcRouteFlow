﻿using System.Collections.Generic;
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

        public State CreateState(string cookie, string path)
        {
            if (GetState(cookie) != null)
            {
                RemoveState(cookie);
            }
            var state = new State()
                           {
                               SessionCookie = cookie,
                               Path = path,
                               Step = 1,
                               CorrelationIds = new Dictionary<string, object>()
                           };
            States.Add(state);
            return state;
        }

        public void CompleteStep(string id)
        {
            var state = GetState(id);
            if (state != null)
            {
                state.StepCompleted = state.Step;
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
            state.StepCompleted = step - 1;
            state.Step = step;
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