using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using MvcRouteFlow.Exceptions;

namespace MvcRouteFlow
{
    public class PathManager
    {

        private static readonly Dictionary<string, object> Paths;

        static PathManager()
        {
            Paths = new Dictionary<string, object>();
        }

        public static void Install(IPath path)
        {


            path.Steps.ForEach(s =>
                                   {
                                       if (!string.IsNullOrEmpty(s.Previous))
                                       {
                                           var step = path.Steps.FirstOrDefault(x => x.Name == s.Previous);
                                           if (step == null)
                                               throw new RouteFlowException(
                                                   string.Format("RouteFlow Init:On Path ({0}) the Step referenced in Step.After not found ({1})", path, s.Previous));
                                           step.Next = s.Name;
                                       }
                                   });

            Paths.Add(path.Name, path);

           

        }

        public static void Remove(IPath path)
        {
            Paths.Remove(path.Name);
        }

        public static Endpoint GetStartingEndpoint(string path)
        {
            var pathInstance = ((IPath) Paths.FirstOrDefault(x => x.Key == path).Value);

            if (pathInstance == null)
                throw new RouteFlowException(string.Format(
                       "RouteFlow:Path ({0}) not installed", path));

            var stepInstance = pathInstance.Steps.FirstOrDefault();

            if (stepInstance == null)
                throw new RouteFlowException(string.Format(
                        "RouteFlow:Cannot find Starting Point on Path ({0})", path));

            return GetEndpoint(path, stepInstance);
        }

        public static Endpoint GetEndpointByName(string path, string name)
        {
            var step = GetStepByName(path, name);
            return GetEndpoint(path, step);
        }

        public static Endpoint GetNextEndpoint(string path, string controller, string action)
        {
            var nextStep = GetStepByName(path, GetStepByControllerAndAction(path, controller, action).Next);
            return GetEndpoint(path, nextStep);
        }

        private static Endpoint GetEndpoint (string path, IStep step)
        {

            Endpoint next = null;

            
            if (step is Interstitial)
            {
                return new Endpoint()
                {
                    Step = step,
                    StepName = step.Name,
                    Controller = "RouteFlow",
                    Action = "Interstitial",
                    Area = "",
                               RouteValues = new 
                                   {
                                       Message = ((Interstitial)step).Message,
                                       Question = ((Interstitial)step).Question,
                                       YesRoute = ((Interstitial)step).YesRoute,
                                       YesLabel = ((Interstitial)step).YesLabel,
                                       NoRoute = ((Interstitial)step).NoRoute,
                                       NoLabel = ((Interstitial)step).NoLabel,
                                   }
                };
            }

            return new Endpoint()
                        {
                            Step = step,
                            StepName = step.Name,
                            Controller = ((ISimpleStep)step).Controller,
                            Action = ((ISimpleStep)step).Action,
                            Correlations = ((ISimpleStep)step).Correlations
                        };
            
        }

        private static IStep GetStepByControllerAndAction(string path, string controller, string action)
        {
            
            var step = ((IPath) Paths.FirstOrDefault(x => x.Key == path).Value)
                .Steps.Where(x => x is SimpleStep)
                .First(s => ((SimpleStep) s).Controller == controller && ((SimpleStep) s).Action == action);

            if (step == null)
                throw new RouteFlowException(string.Format(
                        "RouteFlow:You requested next, but there were no more steps after Path ({0}) Action ({1}/{2})", path, controller, action));

            return step;

        }

        private static IStep GetStepByName(string path, string name)
        {
            var step = ((IPath) Paths.FirstOrDefault(x => x.Key == path).Value)
               .Steps.FirstOrDefault(x => x.Name == name);
            if (step == null)
                throw new RouteFlowException(string.Format(
                        "RouteFlow:You requested a step by name, but it was not found in Path ({0}) Action ({1})", path, name));
            return step;
        }

        public static void Initialize(Assembly assembly)
        {
            // http://stackoverflow.com/questions/15844380/scan-for-all-actions-in-the-site


            var handlers = assembly.GetTypes().Where(type => type.GetInterfaces()
                .Contains(typeof(IHandleRouteFlowInitialization)))
                .Select(x => Activator.CreateInstance(x) as IHandleRouteFlowInitialization).ToList();
            handlers.ForEach(x => x.Setup());

        }

    }

}