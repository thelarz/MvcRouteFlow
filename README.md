MvcRouteFlow
============
MvcRouteFlow **will be** a [NuGet](http://nuget.org/) package that allows you to link ASP.NET MVC Routes together into a workflow using attributes on the Controller Action.

## Current Limitations ##

* State is stored in a static variable and will be destroyed if the app pool recycles. Boom.

## Integrating MvcRouteFlow into your web project

### Register MvcRouteFlow in global.asax.vb

	protected void Application_Start()
    {
        // ... other registrations ...
        MvcRouteFlowConfig.RegisterRouteFlow();
    }

## Decorating your Controller Actions ##

Here's a sample controller (also available in the repo).

	public class TestController : Controller
    {
        
        public ActionResult Index()
        {
            return RouteFlow.Begin("create-article");
        }

        [RouteFlow(Path = "create-article", Step = 1, Select = When.Auto)]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost]
        [RouteFlow(Path = "create-article", Step = 1, Select = When.After, Question = "Thanks for completing step one, would you like to move on?")]
        public ActionResult Page1(string post)
        {
            return RouteFlow.Next();
        }

        [RouteFlow(Path = "create-article", Step = 2, Select = When.Yes)]
        public ActionResult Page2()
        {
            return View();
        }

        [RouteFlow(Path = "create-article", Step = 2, Select = When.No)]
        public ActionResult Page2No()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next();
        }

        [RouteFlow(Path = "create-article", Step = 3, Select = When.Auto)]
        public ActionResult Page3()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next();
        }

        [RouteFlow(Path = "create-article", Select = When.Done)]
        public ActionResult Done()
        {
            return View();
        }

    }

### Select Directives ###

* Auto - Selected when the current step matches.
* After - This directive tells RouteFlow to render an interstitial and as the user a question.
* Yes - Selected when a user selects the YES option on a RouteFlow interstitial.
* No - Selected when a user selects the NO option on a RouteFlow interstitial.
* Done - Selected when the last step is completed.


## Controlling RouteFlow ##

#### Starting a RouteFlow workflow ####

Currently, there's only one way to begin a workflow

	public ActionResult Index()
    {
        return RouteFlow.Begin("create-multi-step-widget");
    }


#### Moving to the next workflow step ####

Moving to the next step involves only telling RouteFlow to do it. Easy user that begins a workflow gets assigned a unique id and their progress is managed by the RouteFlow engine until the workflow completes.

	public ActionResult Index()
    {
        return RouteFlow.Next();
    }


#### Manually stopping a workflow ####

	public ActionResult Index()
    {
        return RouteFlow.Done();
    }

And that's it! That's the nuts and bolts of executing a workflow.


## Change history

### 1.0 (02/26/2014)

* Initial release
