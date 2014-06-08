using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcRouteFlow
{
    public class InterstitialViewModel
    {
        public string Question { get; set; }
        public string Message { get; set; }
        public string YesRoute { get; set; }
        public string YesLabel { get; set; }
        public string NoRoute { get; set; }
        public string NoLabel { get; set; }
    }
}
