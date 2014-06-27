using System.Collections.Generic;

namespace MvcRouteFlow
{
    public class Endpoint
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public object RouteValues { get; set; }
        public List<Correlation> Correlations { get; set; }
        public string StepName { get; set; }
        public IStep Step { get; set; }
    }
}