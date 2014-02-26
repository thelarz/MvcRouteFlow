MvcRouteFlow
============

MvcRouteFlow **will be** a [NuGet](http://nuget.org/) package that allows you to link ASP.NET MVC Routes together into a workflow using attributes on the Controller Action.

## Integrating MvcRouteFlow into your web project

### Register MvcRouteFlow in global.asax.vb

	protected void Application_Start()
    {
        // ... other registrations ...
        MvcRouteFlowConfig.RegisterRouteFlow();
    }

## Decorating your Controller Actions ##

Here's a sample controller (also available in the repo).

	public ActionResult Index()
    {
        return RouteFlow.Begin("create-article");
    }

    [RouteFlow(Path = "create-multi-step-widget", Id = 1, Select = When.Auto)]
    public ActionResult Page1()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Page1(string post)
    {
        return RouteFlow.Next();
    }

    [RouteFlow(Path = "create-multi-step-widget", Id = 2, Select = When.Auto)]
    public ActionResult Page2()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Page2(string post)
    {
        return RouteFlow.Next();
    }

    [RouteFlow(Path = "create-multi-step-widget", Id = 3, Select = When.Auto)]
    public ActionResult Page3()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Page3(string post)
    {
        return RouteFlow.Next();
    }

    [RouteFlow(Path = "create-multi-step-widget", Select = When.Done)]
    public ActionResult Done()
    {
        return View();
    }


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
