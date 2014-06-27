using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MvcRouteFlow
{

    public class State
    {
        
        public string SessionCookie { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> CorrelationIds { get; set; }
        public StateEntry Current { get; set; }
        
        
        public State(string cookie, string path)
        {
            SessionCookie = cookie;
            Path = path;
            CorrelationIds = new Dictionary<string, object>();
            Current = new StateEntry();
        }

    }

    public class StateEntry
    {
        public string Key { get; set; }
        public ActionExecutingContext Context { get; private set; }
        public string Name { get; set; }
        public bool AtStart { get; private set; }

        public void SetContext(ActionExecutingContext context)
        {
            Context = context;
            AtStart = false;
        }

        public string Controller 
        { 
            get
            {
                return (string)Context.RouteData.Values["controller"];
            } 
        }
        public string Action
        {
            get
            {
                return (string)Context.RouteData.Values["action"];
            }
        }
        public string SkipTo { get; set; }

    }
}