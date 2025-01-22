// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class CreateRouteModel
    {
        public CreateRouteModel(PlannedRoute plannedRoute)
        {
            if (plannedRoute.World == null || string.IsNullOrEmpty(plannedRoute.World.Id))
            {
                throw new ArgumentException("Planned route does not have a world set");
            }

            if (string.IsNullOrEmpty(plannedRoute.Name))
            {
                throw new ArgumentException("Planned route does not have a name set");
            }

            if (string.IsNullOrEmpty(plannedRoute.ZwiftRouteName))
            {
                throw new ArgumentException("Planned route does not have the Zwift route name set");
            }

            World = plannedRoute.World.Id;
            Name = plannedRoute.Name;
            ZwiftRouteName = plannedRoute.ZwiftRouteName;
            IsLoop = plannedRoute.IsLoop;
            Serialized = RouteStoreToDisk.SerializeAsJson(plannedRoute, Formatting.None);
            Ascent = (decimal)plannedRoute.Ascent;
            Descent = (decimal)plannedRoute.Descent;
            Distance = (decimal)Math.Round(plannedRoute.Distance / 1000, 1, MidpointRounding.AwayFromZero);
        }

        public string Serialized { get; }

        public bool IsLoop { get; }

        public string ZwiftRouteName { get; }

        public string Name { get; }

        public string World { get; }
        public decimal Distance { get; private set; }
        public decimal Ascent { get; private set; }
        public decimal Descent { get; private set; }
    }
}