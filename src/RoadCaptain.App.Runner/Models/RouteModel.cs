using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.App.Runner.Models
{
    public class RouteModel
    {
        public static RouteModel From(PlannedRoute? plannedRoute, List<Segment> segments)
        {
            var model = new RouteModel
            {
                PlannedRoute = plannedRoute,
            };

            if (plannedRoute == null)
            {
                return model;
            }

            var metrics = plannedRoute
                .RouteSegmentSequence
                .Join(segments,
                    seq => seq.SegmentId,
                    segment => segment.Id,
                    (seq, segment) => segment)
                .Select(segment => new
                {
                    segment.Distance,
                    segment.Ascent,
                    segment.Descent
                })
                .ToList();

            model.TotalDistance = Math.Round(metrics.Sum(s => s.Distance) / 1000, 1).ToString("0.0") + "km";
            model.TotalAscent = Math.Round(metrics.Sum(s => s.Ascent), 1).ToString("0.0") + "m";
            model.TotalDescent = Math.Round(metrics.Sum(s => s.Descent), 1).ToString("0.0") + "m";

            return model;
        }

        public PlannedRoute? PlannedRoute { get; private init; }

        public string? ZwiftRouteName => PlannedRoute?.ZwiftRouteName;

        public SportType Sport => PlannedRoute?.Sport ?? SportType.Unknown;

        public World? World => PlannedRoute?.World;
        public string? Name => PlannedRoute?.Name;
        public string TotalDistance { get; private set; }
        public string TotalAscent { get; private set; }
        public string TotalDescent { get; private set; }
        public bool IsLoop => PlannedRoute?.IsLoop ?? false;
    }
}
