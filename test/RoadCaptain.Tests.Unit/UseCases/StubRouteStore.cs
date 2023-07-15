// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class StubRouteStore : IRouteStore
    {
        public PlannedRoute LoadFrom(string path)
        {
            if (path == "someroute.json")
            {
                return new PlannedRoute();
            }

            if (StoredRoutes.TryGetValue(path, out var plannedRoute))
            {
                return plannedRoute;
            }

            throw new Exception("Route not fond");
        }

        public Task Store(PlannedRoute route, string path)
        {
            StoredRoutes.Add(path, route);
            return Task.CompletedTask;
        }

        public Dictionary<string, PlannedRoute> StoredRoutes { get; } = new();
    }
}
