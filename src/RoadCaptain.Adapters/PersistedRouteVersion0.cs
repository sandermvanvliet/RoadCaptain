// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion0
    {
        public string? ZwiftRouteName { get; set; }
        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        public PlannedRoute AsRoute(World watopia)
        {
            var plannedRoute = new PlannedRoute
            {
                Name = ZwiftRouteName, // original versions did not have a name for the route itself
                ZwiftRouteName = ZwiftRouteName,
                World = watopia,
                Sport = SportType.Cycling
            };

            plannedRoute.RouteSegmentSequence.AddRange(RouteSegmentSequence);
            
            return plannedRoute;
        }
    }
}
