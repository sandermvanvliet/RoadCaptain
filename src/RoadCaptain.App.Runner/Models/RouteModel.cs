// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Runner.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoadCaptain.App.Runner.Models
{
    public class RouteModel
    {
        public static RouteModel From(PlannedRoute? plannedRoute, List<Segment> segments, List<Segment> markers)
        {
            var model = new RouteModel
            {
                PlannedRoute = plannedRoute,
            };

            if (plannedRoute == null)
            {
                return model;
            }

            plannedRoute.CalculateMetrics(segments);

            model.TotalDistance = Math.Round(plannedRoute.Distance / 1000, 1).ToString("0.0") + "km";
            model.TotalAscent = Math.Round(plannedRoute.Ascent, 1).ToString("0.0") + "m";
            model.TotalDescent = Math.Round(plannedRoute.Descent, 1).ToString("0.0") + "m";

            model.Markers = PlannedRoute
                .CalculateClimbMarkers(
                    markers.Where(m => m.Type == SegmentType.Climb || m.Type == SegmentType.Sprint).ToList(),
                    plannedRoute.TrackPoints.ToImmutableArray())
                .Select(marker => new MarkerViewModel(marker.Segment))
                .ToList();

            return model;
        }

        public PlannedRoute? PlannedRoute { get; private init; }

        public string? ZwiftRouteName => PlannedRoute?.ZwiftRouteName;

        public SportType Sport => PlannedRoute?.Sport ?? SportType.Unknown;

        public World? World => PlannedRoute?.World;
        public string? Name => PlannedRoute?.Name;
        public string TotalDistance { get; private set; } = "0";
        public string TotalAscent { get; private set; } = "0";
        public string TotalDescent { get; private set; } = "0";
        public bool IsLoop => PlannedRoute?.IsLoop ?? false;
        public List<MarkerViewModel> Markers { get; private set; } = new();
    }
}

