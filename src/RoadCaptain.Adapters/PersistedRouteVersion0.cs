using System.Collections.Generic;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion0
    {
        public string ZwiftRouteName { get; set; }
        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        public PlannedRoute AsRoute(World watopia)
        {
            var plannedRoute = new PlannedRoute
            {
                Name = ZwiftRouteName, // original versions did not have a name for the route itself
                ZwiftRouteName = ZwiftRouteName,
                World = watopia,
                Sport = SportType.Bike
            };

            plannedRoute.RouteSegmentSequence.AddRange(RouteSegmentSequence);
            
            return plannedRoute;
        }
    }
}