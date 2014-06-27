using System.Collections.Generic;
using System.Linq;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{

    public interface IPath
    {
        string Name { get; set; }
        List<IStep> Steps { get; set; }
        IPath AddStep(IStep step);
        IPath Install();
        void Remove();

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
        
        public IPath AddStep(IStep step)
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
            if (Steps == null)
            {
                throw new RouteFlowException(string.Format("RouteFlow:No steps found on Path ({0})", this.Name));
            }
            PathManager.Install(this);
            return this;
        }

        public void Remove()
        {
            PathManager.Remove(this);
        }

    }
}