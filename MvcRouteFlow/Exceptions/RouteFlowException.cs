using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcRouteFlow.Exceptions
{
    public class RouteFlowException : Exception
    {
        public RouteFlowException()
            : base()
        {
        }
        public RouteFlowException(string message)
            : base(message)
        {
        }
        public RouteFlowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
