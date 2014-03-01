using System.Collections.Generic;

namespace MvcRouteFlow
{
    public class State
    {
        public string SessionCookie { get; set; }
        public string Path { get; set; }
        public int Step { get; set; }
        public int StepCompleted { get; set; }
        public int StepOnBeforeCompleted { get; set; }
        public Dictionary<string, object> CorrelationIds { get; set; }
    }
}