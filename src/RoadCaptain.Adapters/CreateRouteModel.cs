// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.Adapters
{
    internal class CreateRouteModel
    {
        public CreateRouteModel(PlannedRoute plannedRoute, List<Segment> segments)
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
            CalculateTotalAscentAndDescent(plannedRoute, segments);
        }

        private void CalculateTotalAscentAndDescent(PlannedRoute route, List<Segment> segments)
        {
            double totalAscent = 0;
            double totalDescent = 0;
            double totalDistance = 0;

            foreach (var sequence in route.RouteSegmentSequence)
            {
                var segment = segments.Single(s => s.Id == sequence.SegmentId);

                if (sequence.Direction == SegmentDirection.AtoB)
                {
                    totalAscent += segment.Ascent;
                    totalDescent += segment.Descent;
                }
                else
                {
                    totalAscent += segment.Descent;
                    totalDescent += segment.Ascent;
                }

                totalDistance += segment.Distance;
            }

            Distance = (decimal)Math.Round(totalDistance / 1000, 1);
            Ascent = (decimal)totalAscent;
            Descent = (decimal)totalDescent;
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