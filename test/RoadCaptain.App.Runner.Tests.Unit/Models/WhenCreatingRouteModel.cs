// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Models
{
    public class WhenCreatingRouteModel
    {
        private readonly List<Segment> _segments = new();
        private readonly List<Segment> _markers = new();

        public WhenCreatingRouteModel()
        {
            var trackPointSeg1Point1 = new TrackPoint(1, 2, 0);
            var trackPointSeg1Point2 = trackPointSeg1Point1.ProjectTo(90, 100, 100);

            _segments.Add(new Segment(new List<TrackPoint>
            {
                trackPointSeg1Point1,
                trackPointSeg1Point2
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
            var result = Runner.Models.RouteModel.From(null, _segments, _markers);

            result
                .PlannedRoute
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalDistanceIsLengthOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1",
                type: SegmentSequenceType.Regular, direction: SegmentDirection.AtoB, index: 0));

            var result = Runner.Models.RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalDistance
                .Should()
                .Be("0.1km");
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalAscentIsAscentOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1",
                type: SegmentSequenceType.Regular, direction: SegmentDirection.AtoB, index: 0));

            var result = Runner.Models.RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalAscent
                .Should()
                .Be("100.0m");
        }

        [Fact]
        public void GivenPlannedRouteWithOneSegment_TotalDescentIsDescentOfSegment()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1",
                type: SegmentSequenceType.Regular, direction: SegmentDirection.AtoB, index: 0));

            var result = Runner.Models.RouteModel.From(plannedRoute, _segments, _markers);

            result
                .TotalDescent
                .Should()
                .Be("0.0m");
        }

        [Fact]
        public void GivenPlannedRouteWithMarkerOnRoute_ResultingModelContainsMarker()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1",
                type: SegmentSequenceType.Regular, direction: SegmentDirection.AtoB, index: 0));

            var result = Runner.Models.RouteModel.From(plannedRoute, _segments, _markers);

            result
                .Markers
                .Should()
                .Contain(m => m.Name == "marker-1");
        }


        [Fact]
        public void GivenPlannedRouteWithSegmentThatDoesntExist_MissingSegmentExceptionIsThrown()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-2",
                type: SegmentSequenceType.Regular, direction: SegmentDirection.AtoB, index: 0));

            Action action = () => Runner.Models.RouteModel.From(plannedRoute, _segments, _markers);

            action
                .Should()
                .Throw<MissingSegmentException>();
        }
    }
}
