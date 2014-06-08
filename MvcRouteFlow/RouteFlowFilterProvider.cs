using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcRouteFlow
{

    // May use this to possible inject filters onto routeflow actions

    //public class RouteFlowFilterProvider : IFilterProvider
    //{
    //    public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    //    {
    //        var filters = FiltersSettings.Settings.Filters;
    //        foreach (var filter in filters.Cast<FilterAction>())
    //        {
    //            var filterType = Type.GetType(filter.Type);
    //            yield return new Filter(
    //                    Activator.CreateInstance(filterType),
    //                    FilterScope.Global, order: null
    //                );
    //        }
    //    }
    //}

    


}
