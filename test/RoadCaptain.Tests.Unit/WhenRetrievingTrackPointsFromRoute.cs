// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenRetrievingTrackPointsFromRoute
    {
        [Fact]
        public void GivenAPlannedRouteOf800Meters_DistanceOnSegmentOnLastTrackPointIs800Meters()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            plannedRoute.TrackPoints[^1].DistanceOnSegment.Should().BeApproximately(800, 1);
        }

        [Fact]
        public void GivenAPlannedRouteOf800Meters_DistancePropertyIsSetTo800Meters()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            plannedRoute.Distance.Should().BeApproximately(800, 1);
        }

        [Fact]
        public void GivenAPlannedRouteOf800Meters_TrackPointsPropertyContainsAllTrackPoints()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            plannedRoute.TrackPoints.Should().HaveCount(9);
        }

        [Fact]
        public void GivenAPlannedRoute_TrackPointIndexesAreSequential()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            for (var index = 0; index < plannedRoute.TrackPoints.Count; index++)
            {
                plannedRoute.TrackPoints[index].Index.Should().Be(index, $"TrackPoint at index {index} should have that value");
            }
        }

        [Fact]
        public void GivenAPlannedRouteWithASegmentInReverseDirection_TrackPointIndexesAreSequential()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2-rev", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            for (var index = 0; index < plannedRoute.TrackPoints.Count; index++)
            {
                plannedRoute.TrackPoints[index].Index.Should().Be(index, $"TrackPoint at index {index} should have that value");
            }
        }

        [Fact]
        public void GivenAPlannedRouteWithTheSameSegmentAppearingMultipleTimes_TrackPointIndexesAreSequential()
        {
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-2", "segment-2", "segment-3");

            plannedRoute.CalculateMetrics(segments);

            for (var index = 0; index < plannedRoute.TrackPoints.Count; index++)
            {
                plannedRoute.TrackPoints[index].Index.Should().Be(index, $"TrackPoint at index {index} should have that value");
            }
        }

        private static List<Segment> CreateSegments()
        {
            var segment1Point1 = new TrackPoint(0, 0, 0, ZwiftWorldId.Watopia);
            var segment1Point2 = segment1Point1.ProjectTo(90, 100, 20);
            var segment1Point3 = segment1Point2.ProjectTo(90, 100, 20);

            var segment2Point1 = segment1Point3.ProjectTo(90, 100, 90);
            var segment2Point2 = segment2Point1.ProjectTo(90, 100, 100);
            var segment2Point3 = segment2Point2.ProjectTo(90, 100, 90);

            var segment3Point1 = segment2Point3.ProjectTo(90, 100, 75);
            var segment3Point2 = segment3Point1.ProjectTo(90, 100, 70);
            var segment3Point3 = segment3Point2.ProjectTo(90, 100, 50);

            var segments = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    segment1Point1,
                    segment1Point2,
                    segment1Point3
                })
                {
                    Id = "segment-1",
                    Name = "Segment 1"
                },
                new(new List<TrackPoint>
                {
                    segment2Point1,
                    segment2Point2,
                    segment2Point3
                })
                {
                    Id = "segment-2",
                    Name = "Segment 2"
                },
                new(new List<TrackPoint>
                {
                    segment3Point1,
                    segment3Point2,
                    segment3Point3
                })
                {
                    Id = "segment-3",
                    Name = "Segment 3",
                },
            };

            foreach (var segment in segments)
            {
                segment.Type = SegmentType.Segment;
                segment.Sport = SportType.Cycling;
                segment.CalculateDistances();
            }

            return segments;
        }
        
        private static PlannedRoute CreatePlannedRoute(params string[] segmentIds)
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia, Name = "Watopia" },
                WorldId = "watopia",
                Sport = SportType.Cycling,
                ZwiftRouteName = "Test Zwift Route",
                Name = "RoadCaptain Test Route"
            };

            foreach (var segmentId in segmentIds)
            {
                var direction = segmentId.EndsWith("-rev")
                    ? SegmentDirection.BtoA
                    : SegmentDirection.AtoB;

                plannedRoute.RouteSegmentSequence.Add(
                    new SegmentSequence(
                        segmentId.Replace("-rev", ""),
                        direction,
                        SegmentSequenceType.Regular));
            }

            return plannedRoute;
        }
    }
}

