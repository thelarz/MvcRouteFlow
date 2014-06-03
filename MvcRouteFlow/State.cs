using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MvcRouteFlow
{
    public class State
    {
        
        public string SessionCookie { get; set; }
        public string Path { get; set; }
        public int MaxSteps { get; set; }
        public Dictionary<string, object> CorrelationIds { get; set; }
        public bool MovingForward { get; set; }

        public Stack<StateEntry> Entries = new Stack<StateEntry>();
        public StateEntry Current { get; set; }

        public int LastCompletedStep
        {
            get 
            { 
                // using FirstOrDefault because stacks work in reverse.
                var lastcompleted = Entries.FirstOrDefault(x => x.Completed);
                return lastcompleted == null ? 0 : lastcompleted.Step;
            }
        }

        public void Next()
        {
            Next(0);
        }

        public void Next(int skip)
        {
            
            MovingForward = true;

            var nextStep = Current.Step + skip + 1;

            //this code is failing because it's looking to increment from step 3 -> 10, but there's a step 4,5,6 etc. in this path.

            var endpoint = PathManager.GetEndpoint(Path, nextStep);
            if (!endpoint.IsPassThru)
            {
                Entries.Push(new StateEntry()
                                 {
                                     Step = endpoint.StepId,
                                     Key = endpoint.Key,
                                 });
            }
            Current = new StateEntry()
            {
                Step = endpoint.StepId,
                Key = endpoint.Key,
            };

        }

        public State(string cookie, string path)
        {
            SessionCookie = cookie;
            Path = path;
            MaxSteps = PathManager.GetMaxSteps(path);
            CorrelationIds = new Dictionary<string, object>();
            MovingForward = true;
            
            Current = new StateEntry() {Step = 1};

            Entries.Push(new StateEntry()
                             {
                                 Step = 1,
                             });
        }

    }

    public class StateEntry
    {
        public int Step { get; set; }
        public bool Completed { get; set; }
        public bool OnBeforeCompleted { get; set; }
        public bool IsPassThru { get; set; }
        public string Key { get; set; }
    }
}