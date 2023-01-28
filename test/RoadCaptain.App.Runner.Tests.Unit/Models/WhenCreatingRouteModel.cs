// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentAssertions;
using RoadCaptain.App.Runner.Models;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Models
{
    public class WhenCreatingRouteModel
    {
        private List<Segment> _segments = new();
        private List<Segment> _markers = new();

        public WhenCreatingRouteModel()
        {
            _segments.Add(new Segment(new List<TrackPoint>
            {
                new(1, 2, 0) {DistanceFromLast = 0, DistanceOnSegment = 0, Index = 0},
                new(1, 3, 100) { DistanceFromLast = 100, DistanceOnSegment = 100, Index = 1},
            })
            {
                Id = "seg-1",
                Sport = SportType.Cycling,
                Type = SegmentType.Segment,
                Name = "seg-1"
            });

            _markers.Add(
                new Segment(new List<TrackPoint>
                {
                    new (1, 2, 0),
                    new (1, 2.0001, 0)
                })
                {
                    Id = "marker-1",
                    Name = "marker-1",
                    Type = SegmentType.Sprint,
                    Sport = SportType.Cycling
                });

            // To prevent issues with culture changing the decimal separator
            // and breaking the tests when running on different machines.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public void GivenPlannedRouteIsNull_ModelWithoutRouteIsReturned()
        {
            var result = RouteModel.From(null, _segments, _markers);

            result
                .PlannedRoute
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalDistanceIsLengthOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", Type = SegmentSequenceType.Regular, Direction = SegmentDirection.AtoB, Index = 0 });

            var result = RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalDistance
                .Should()
                .Be("0.1km");
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalAscentIsAscentOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", Type = SegmentSequenceType.Regular, Direction = SegmentDirection.AtoB, Index = 0 });

            var result = RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalAscent
                .Should()
                .Be("100.0m");
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalDescentIsDescentOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", Type = SegmentSequenceType.Regular, Direction = SegmentDirection.AtoB, Index = 0 });

            var result = RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalDescent
                .Should()
                .Be("0.0m");
        }

        [Fact]
        public void GivenPlannedRouteWithMarkerOnRoute_ResultingModelContainsMarker()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", Type = SegmentSequenceType.Regular, Direction = SegmentDirection.AtoB, Index = 0 });

            var result = RouteModel.From(plannedRoute, _segments, _markers);

            result
                .Markers
                .Should()
                .Contain(m => m.Name == "marker-1");
        }


        [Fact]
        public void GivenPlannedRouteWithSegmentThatDoesntExist_MissingSegmentExceptionIsThrown()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence
            {
                SegmentId = "seg-2", Type = SegmentSequenceType.Regular, Direction = SegmentDirection.AtoB, Index = 0
            });

            Action action = () => RouteModel.From(plannedRoute, _segments, _markers);

            action
                .Should()
                .Throw<MissingSegmentException>();
        }
    }
}
