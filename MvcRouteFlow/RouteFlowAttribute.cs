using System;

namespace MvcRouteFlow
{
    public class RouteFlowAttribute : Attribute
    {
        public string Path { get; set; }
        public int Step { get; set; }
        public When Select { get; set; }
    }
}