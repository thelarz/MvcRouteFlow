using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Microsoft.Web.WebPages.OAuth;
using MvcRouteFlow.Web.Models;

namespace MvcRouteFlow.Web
{

    // Probably incorporate webactivator 

    public static class MvcRouteFlowConfig
    {
        public static void RegisterRouteFlow()
        {
            RouteFlow.Register();
        }


    }
}
