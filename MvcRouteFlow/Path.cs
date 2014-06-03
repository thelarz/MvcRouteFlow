using System.Collections.Generic;

namespace MvcRouteFlow
{

    public enum When
    {
        Auto,
        Yes,
        No,
        Done,
        After,
        Before
    }

    public class Path
    {
        public string Key { get; set; }
        public string StartController { get; set; }
        public string StartAction { get; set; }
        public int MaxSteps { get; set; }
        public List<Step> Steps { get; set; }
    }

    public class Step
    {
        public int Id { get; set; }
        public List<Endpoint> Endpoints { get; set; }
    }
    
    public class Endpoint
    {
        // Key is probably not needed. I only started populating it with the "controller/action".hash recently
        // to attempt to link together all steps on a particular controller method.
        // It's not used.
        public string Key { get; set; }
        public bool BeforeWasVisited { get; set; }
        public When Select { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public bool IsPassThru { get; set; }
        public int StepId { get; set; }
    }

}