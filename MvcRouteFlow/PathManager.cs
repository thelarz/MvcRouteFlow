using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace MvcRouteFlow
{
    public class PathManager
    {

        private static readonly List<Path> Paths;

        static PathManager()
        {
            Paths = new List<Path>();
        }

        public static Endpoint GetStartingEndpoint(string path)
        {
            var endpoint =
                Paths.FirstOrDefault(x => x.Key == path)
                     .Steps.FirstOrDefault(s => s.Id == 1)
                     .Endpoints.FirstOrDefault(e => e.Select == When.Auto);
            return endpoint;
        }

        public static int GetMaxSteps(string path)
        {
            if (!Paths.Any(x => x.Key == path))
                return 0;

            var stepCount =
                Paths.FirstOrDefault(x => x.Key == path)
                     .MaxSteps;
            return stepCount;
        }

        public static Endpoint GetBefore(string path, int step)
        {

            var curr =
                Paths.FirstOrDefault(x => x.Key == path)
                     .Steps.FirstOrDefault(s => s.Id == step);

            if (curr == null)
                return null;

            var endpoint = curr.Endpoints.FirstOrDefault(e => e.Select == When.Before);
            return endpoint;

        }

        public static List<Endpoint> GetYesNoEndpointsForStep(string path, int step)
        {
            var curr =
                Paths.FirstOrDefault(x => x.Key == path)
                     .Steps.FirstOrDefault(s => s.Id == step);

            var endpoints = curr.Endpoints.Where(e => e.Select == When.Yes || e.Select == When.No);
            return endpoints.ToList();

        }

        public static Endpoint GetEndpoint(string path, int step)
        {
            var nextStep =
                Paths.FirstOrDefault(x => x.Key == path)
                     .Steps.OrderBy(x => x.Id).FirstOrDefault(s => s.Id >= step);

            Endpoint next = null;

            if (nextStep == null)
                throw new ApplicationException(string.Format(
                        "RouteFlow: you requested next, there was no more steps after Path [{0}] Step [{1}]", path, step));

            next = nextStep.Endpoints.FirstOrDefault(e => e.Select == When.Auto || e.Select == When.Done);

            if (next == null)
            {
                var done = Paths.FirstOrDefault(x => x.Key == path)
                                .Steps.FirstOrDefault(s => s.Endpoints.Any(x => x.Select == When.Done));

                if (done == null)
                    throw new ApplicationException(string.Format(
                        "RouteFlow: you requested next, but there were no When.Auto or When.Done endpoints on Path [{0}] after Step [{1}]", path, step));

                return done.Endpoints.First(e => e.Select == When.Done);

            }

            return next;

        }

        public static void Initialize(Assembly assembly)
        {
            // http://stackoverflow.com/questions/15844380/scan-for-all-actions-in-the-site


            var controllers = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Controller))).ToList();
            controllers.ForEach((x) => GetActions(x));

        }

        private static void GetActions(Type controller)
        {

            //TODO: Extract to its own class and write some unit tests around this

            // Get a descriptor of this controller
            var controllerDesc = new ReflectedControllerDescriptor(controller);

            // Look at each action in the controller
            foreach (var action in controllerDesc.GetCanonicalActions())
            {
                // Get any attributes (filters) on the action
                var attributes = action.GetCustomAttributes(typeof(RouteFlowAttribute), false).Where(filter => filter is RouteFlowAttribute);

                foreach (var a in attributes)
                {

                    var attr = a as RouteFlowAttribute;

                    if (attr == null)
                        continue;

                    var path = Paths.FirstOrDefault(x => x.Key == attr.Path);
                    if (path == null)
                    {
                        path = new Path()
                                   {
                                       Key = attr.Path,
                                       Steps = new List<Step>()
                                   };
                        Paths.Add(path);
                        
                    }

                    var step = path.Steps.FirstOrDefault(x => x.Id == attr.Step);
                    if (step == null)
                    {
                        step = new Step()
                                   {
                                       Id = attr.Step,
                                       Endpoints = new List<Endpoint>()
                                   };
                        path.Steps.Add(step);
                        path.MaxSteps++;
                    }

                    var item = step.Endpoints.FirstOrDefault(x => x.Select == attr.Select);

                    if (item == null)
                    {
                        step.Endpoints.Add(new Endpoint()
                                               {
                                                   Key = string.Format("{0}/{1}", controllerDesc.ControllerName, action.ActionName).GetHashCode().ToString(),
                                                   Select = attr.Select,
                                                   Controller = controllerDesc.ControllerName,
                                                   Action = action.ActionName,
                                                   Label = attr.Label,
                                                   IsPassThru = attr.PassThru,
                                                   StepId = step.Id
                                               });
                    }

                }

            }

#if DEBUG
            foreach (var path in Paths)
            {
                Trace.WriteLine(string.Format("Path: {0}", path.Key));
                foreach (var step in path.Steps)
                {
                    Trace.WriteLine(string.Format("Step: {0}/{1}", path.Key, step.Id));
                    foreach (var endpoint in step.Endpoints)
                    {
                        Trace.WriteLine(string.Format("Endp: {0}/{1}/{2}/{3}/{4}", path.Key, step.Id, endpoint.Controller, endpoint.Action, endpoint.Select.ToString()));
                    }
                }
            }
#endif



        }

    }
}