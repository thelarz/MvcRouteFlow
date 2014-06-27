using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcRouteFlow
{
    public interface IProvideRouteFlowSession
    {
        string SessionId { get; }
    }
}
