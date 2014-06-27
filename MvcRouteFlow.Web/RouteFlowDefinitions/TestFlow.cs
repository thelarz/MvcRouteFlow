using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcRouteFlow.Web.RouteFlowDefinitions
{
    public class TestFlow : IHandleRouteFlowInitialization
    {
        public void Setup()
        {
            var flow = new Path<TestFlow>()

                .AddStep(new SimpleStep()
                             {
                                 Name = "page-1",
                                 Controller = "Test",
                                 Action = "Page1"
                             })
                .AddStep(new SimpleStep()
                             {
                                 Name = "page-2",
                                 Controller = "Test",
                                 Action = "Page2"
                             }.After("page-1"))
                .AddStep(new SimpleStep()
                            {
                                Name = "page-3",
                                Controller = "Test",
                                Action = "Page3"
                            }.After("page-2"))
                .AddStep(new SimpleStep()
                            {
                                Name = "page-4",
                                Controller = "Test",
                                Action = "Page4"
                            }.After("page-4"))
                .Install();

        }

        public void TearDown()
        {
            throw new NotImplementedException();
        }
    }
}