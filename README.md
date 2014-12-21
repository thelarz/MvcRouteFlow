MvcRouteFlow
============
MvcRouteFlow is a [NuGet](http://nuget.org/) package that allows you to link ASP.NET MVC Routes together into a workflow using attributes on the Controller Action.

## Current Limitations ##

* State is stored in a static variable and will be destroyed if the app pool recycles. Boom.
* Uses SessionId to identify the state. One state per SessionId at one time.

## Integrating MvcRouteFlow into your web project

### Register MvcRouteFlow in global.asax.cs

    protected void Application_Start()
    {
        // ... other registrations ...
        RouteFlow.Register();
    }

## Sample RouteFlow Definition ##

This sample routeflow definition describes a 4 step workflow using the TestController. It assumes you have already setup the TestController and all appropriate views.

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
                .Install();

        }

        public void TearDown()
        {
            throw new NotImplementedException();
        }
    }


## Sample Controller ##

Here's a sample controller (also available in the repo).

    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(System.Web.Mvc.ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            filterContext.HttpContext.Response.Expires = -1;
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoServerCaching();
            //filterContext.HttpContext.Response.Cache.SetAllowResponseInBrowserHistory(false);
            //filterContext.HttpContext.Response.CacheControl = "no-cache";
            filterContext.HttpContext.Response.Cache.SetNoStore();

        }
    }

    public class TestController : Controller
    {
        
        [RouteFlow]
        public ActionResult Index()
        {
            return RouteFlow.Begin<TestFlow>();
        }

        [NoCache]
        [RouteFlow]
        public ActionResult Page1()
        {
            return View();
        }

        [HttpPost, NoCache]
        public ActionResult Page1(string id)
        {
            return RouteFlow.Next();
        }

        [NoCache]
        [RouteFlow]
        public ActionResult Page2()
        {
            return View();
        }

        [HttpPost, NoCache]
        public ActionResult Page2(string post)
        {
            return RouteFlow.Next();
        }

        [NoCache]
        [RouteFlow]
        public ActionResult Page3()
        {
            return View();
        }

        [HttpPost, NoCache]
        public ActionResult Page3(string post)
        {
            return RouteFlow.Next();
        }

    }

### RouteFlow Attributes/Filters ###

***RouteFlow*** - Main Attribute/Filter to control flow of controller actions.

***RouteFlowSetCorrelation*** - Save a correlation item into the RouteFlow state manager. These are grabbed from the 	Controller Methods post values and copied to state for later retrieval.

	[HttpPost]
        [RouteFlowSetCorrelation(Path = typeof(ToDoFlow), Value = "itemid", As = "todoitem")]
        public ActionResult Edit(int itemid, string description)
        ...
 
***RouteFlowGetCorrelation*** - Get a correlation item from the RouteFlow state manager. When you ***GET*** a value it is copied into a Controller Actions parameters.

        [RouteFlowGetCorrelation(Path = typeof(ToDoFlow), Name = "todoitem", AssignTo = "itemid")]
        public ActionResult Step2OfToDo(int? itemid)
        ...


### RouteFlow Built-in Interstitials ###

        .AddStep(new Interstitial()
        {
            Name = "add-todo-attachments",
            Question = "Would you like to Add attachments to this TODO item?",
            YesRoute = "add-attachments",
            YesLabel = "Yes",
            NoRoute = "finish-todo",
            NoLabel = "No"
        }
        .After("{some previous step"))

## Controlling RouteFlow ##

#### Starting a RouteFlow workflow ####

There's only one way to begin a workflow

    public ActionResult Index()
    {
        return RouteFlow.Begin<ToDoFlow>();
    }


#### Moving to the next workflow step ####

Moving to the next step involves only telling RouteFlow to do it. Every user that begins a workflow gets assigned a unique id and their progress is managed by the RouteFlow engine until the workflow completes.

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


## Handling Exceptions ##

You can add a filter similar to this one to show a particular view in the event an error occurs during RouteFlow processing.

    public class RouteFlowExceptionAttribute : System.Web.Mvc.HandleErrorAttribute
    {

        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {

            if (filterContext.ExceptionHandled)
            {
                return;
            }
                
            if (filterContext.Exception is RouteFlowException)
            {
                filterContext.Result = new ViewResult
                                            {
                                                ViewName = "RouteFlowException"
                                            };
            }
            else
            {
                return;
            }

            filterContext.ExceptionHandled = true;

        }
    }

You'll want to register it in your global.asax.cs as well

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
      	    ... other filters
            filters.Add(new RouteFlowExceptionAttribute());
        }


## Change history

### 1.0 (02/26/2014)

* Initial release
