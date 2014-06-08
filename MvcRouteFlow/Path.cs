using System.Collections.Generic;

namespace MvcRouteFlow
{

    public interface IPath
    {
        string Name { get; set; }
        List<IStep> Steps { get; set; }
    }

    public class Path<T> : IPath
    {

        public string Name { get; set; }
        public List<IStep> Steps { get; set; }

        private int _lastId = 1;

        public Path()
        {
            Name = typeof (T).Name;
        }
        
        public Path<T> AddStep(IStep step)
        {
            if (Steps == null)
            {
                Steps = new List<IStep>();
            }
            step.Sequence = _lastId++;
            Steps.Add(step);

            return this;
        }

        public IPath Install()
        {
            PathManager.Install(this);
            return this;
        }

    }

    public enum When
    {
        Auto,
        Yes,
        No,
        Done,
        After,
        Before
    }
    
    public class Endpoint
    {
        
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public object RouteValues { get; set; }
        public List<Correlation> Correlations { get; set; }
        public string StepName { get; set; }

        // Key is probably not needed. I only started populating it with the "controller/action".hash recently
        // to attempt to link together all steps on a particular controller method.
        // It's not used.

        //public string Key { get; set; }
        //public bool BeforeWasVisited { get; set; }
        //public When Select { get; set; }
        //public string Label { get; set; }
        //public bool IsPassThru { get; set; }
        //public int StepId { get; set; }
        
    }

}