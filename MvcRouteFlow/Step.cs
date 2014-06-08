using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcRouteFlow
{
    
    
    public interface IStep
    {
        string Name { get; set; }
        int Sequence { get; set; }
        string Previous { get; set; }
        string Next { get; set; }
        List<Correlation> Correlations { get; } 
    }

    public interface IControllerStep
    {
        string Controller { get; set; }
        string Action { get; set; }
        string Area { get; set; }
        object[] RouteValues { get; set; }
    }

    public interface ISimpleStep : IControllerStep, IStep
    {
        
    }

    public class Correlation
    {
        public string Type { get; set; }
        public string Key { get; set; }
        public string RouteItem { get; set; }
        public bool IsRequired { get; set; }

        public Correlation Put(string value)
        {
            Type = "SET";
            RouteItem = value;
            return this;
        }

        public Correlation Into(string key)
        {
            Key = key;
            return this;
        }

        public Correlation Get(string value)
        {
            Type = "GET";
            RouteItem = value;
            return this;
        }

        public Correlation From(string key)
        {
            Key = key;
            return this;
        }

        public Correlation Required()
        {
            IsRequired = true;
            return this;
        }
    
}

    public class SimpleStep : ISimpleStep
    {
        public string Name { get; set; }
        public int Sequence { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public object[] RouteValues { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
        public List<Correlation> Correlations { get; set; }

        public SimpleStep After(string name)
        {
            Previous = name;
            return this;
        }

        public SimpleStep AddCorrelation(Correlation target)
        {
            Correlations.Add(target);
            return this;
        }

        public SimpleStep GetCorrelation(Correlation target)
        {
            Correlations.Add(target);
            return this;
        }

        public SimpleStep()
        {
            Correlations = new List<Correlation>();
        }
    }

    public class Interstitial : IStep
    {
        public string Name { get; set; }
        public int Sequence { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
        public List<Correlation> Correlations { get; private set; }

        public string Message { get; set; }
        public string Question { get; set; }
        public string YesRoute { get; set; }
        public string YesLabel { get; set; }
        public string NoRoute { get; set; }
        public string NoLabel { get; set; }
        public Interstitial After(string name)
        {
            Previous = name;
            return this;
        }


        
        public Interstitial()
        {
            Correlations = new List<Correlation>();
        }
    }

}
