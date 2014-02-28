MvcRouteFlow
============
MvcRouteFlow **will be** a [NuGet](http://nuget.org/) package that allows you to link ASP.NET MVC Routes together into a workflow using attributes on the Controller Action.

## Current Limitations ##

* State is stored in a static variable and will be destroyed if the app pool recycles. Boom.
* Uses SessionId to identify the state. One state per SessionId at one time.

## Integrating MvcRouteFlow into your web project

### Register MvcRouteFlow in global.asax.cs

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
            return RouteFlow.Begin("test-path");
        }

        [RouteFlow(Path = "test-path", Step = 1, Select = When.Auto)]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost]
        [RouteFlow(Path = "test-path", Step = 1, Select = When.After, 
            Message = "Thanks for completing step one", Question = "Would you like to move on?")]
        public ActionResult Page1(string post)
        {
            return RouteFlow.Next(this, new { id = 22 });
        }

        [RouteFlow(Path = "test-path", Step = 2, Select = When.Yes)]
        public ActionResult Page2()
        {
            return View();
        }

        [RouteFlow(Path = "test-path", Step = 2, Select = When.No, Label = "No thanks")]
        public ActionResult Page2No()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next(this, new { id = 22 });
        }

        [RouteFlow(Path = "test-path", Step = 3, Select = When.Auto)]
        public ActionResult Page3(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next(this);
        }

        [RouteFlow(Path = "test-path", Select = When.Done)]
        public ActionResult Done()
        {
            return View();
        }

    }


### RouteFlow Attributes ###

* Path - Free text RouteFlow path used to link several steps together.
* Step - Control the order the actions appear in the RouteFlow.
* Select - See **Select Directives** below.
* Message - A message you can display to the user on the RouteFlow interstitial.
* Question - Displayed on the RouteFlow interstitial.
* Label - Used for When.Yes/No to label the links on the RouteFlow interstitial.

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
