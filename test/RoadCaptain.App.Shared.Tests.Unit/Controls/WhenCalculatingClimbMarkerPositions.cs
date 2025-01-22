// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.App.Shared.Controls;
using Xunit;

namespace RoadCaptain.App.Shared.Tests.Unit.Controls
{
    public class WhenCalculatingClimbMarkerPositions
    {
        private List<Segment> _markers = null!;
        private List<Segment> _segments = null!;

        public WhenCalculatingClimbMarkerPositions()
        {
            BuildSegmentsAndMarkers();
        }

        private static PlannedRoute CreatePlannedRoute(params string[] segmentIds)
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia, Name = "Watopia" },
                WorldId = "watopia",
                Sport = SportType.Cycling,
                ZwiftRouteName = "Test Zwift Route"
            };

            foreach (var segmentId in segmentIds)
            {
                plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId, SegmentDirection.AtoB,
                    SegmentSequenceType.Regular));
            }

            return plannedRoute;
        }

        [Fact]
        public void GivenRouteWithClimbSegmentInReverse_NoClimbMarkersAreGenerated()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3"), 
                _markers.Where(m => m.Id == "climb-1-rev").ToList(), 
                _segments);

            climbMarkers.Should().BeEmpty();
        }

        [Fact]
        public void GivenRouteWithClimbSegmentForward_SingleClimbMarkerIsGenerated()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3"), 
                _markers.Where(m => m.Id == "climb-1").ToList(), 
                _segments);

            climbMarkers.Should().NotBeEmpty();
            climbMarkers[0].Segment.Id.Should().Be("climb-1");
        }

        [Fact]
        public void GivenRouteCrossingTheSameClimbSegmentForwardTwice_TwoClimbMarkersAreGenerated()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3", "segment-1", "segment-2", "segment-3"), 
                _markers, 
                _segments);

            climbMarkers.Should().HaveCount(2);
            climbMarkers[0].Segment.Id.Should().Be("climb-1");
            climbMarkers[1].Segment.Id.Should().Be("climb-1");
        }

        [Fact]
        public void GivenRouteCrossingTheSameClimbSegmentForwardTwice_TwoClimbMarkersAreGeneratedWithCorrectStartFinishIndexes()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3", "segment-1", "segment-2", "segment-3"), 
                _markers, 
                _segments);

            climbMarkers.Should().HaveCount(2);
            climbMarkers[0].Start.Index.Should().NotBe(climbMarkers[1].Start.Index);
            climbMarkers[0].Finish.Index.Should().NotBe(climbMarkers[1].Finish.Index);
        }

        [Fact]
        public void GivenRouteWithForwardAndReverseClimbSegment_TwoClimbMarkersAreGenerated()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3", "segment-2-rev", "segment-1-rev"), 
                _markers, 
                _segments);

            climbMarkers.Should().HaveCount(2);
            climbMarkers[0].Segment.Id.Should().Be("climb-1");
            climbMarkers[1].Segment.Id.Should().Be("climb-1-rev");
        }

        [Fact]
        public void GivenRouteWithForwardReverseForwardClimbSegments_ThreeClimbMarkersAreGenerated()
        {
            var climbMarkers = CalculateClimbMarkers(
                CreatePlannedRoute("segment-1", "segment-2", "segment-3", "segment-2-rev", "segment-1-rev", "segment-1", "segment-2", "segment-3"), 
                _markers, 
                _segments);

            climbMarkers.Should().HaveCount(3);
            climbMarkers[0].Segment.Id.Should().Be("climb-1");
            climbMarkers[1].Segment.Id.Should().Be("climb-1-rev");
            climbMarkers[2].Segment.Id.Should().Be("climb-1");
        }

        private static List<(Segment Segment, TrackPoint Start, TrackPoint Finish)> CalculateClimbMarkers(PlannedRoute plannedRoute, List<Segment> markers, List<Segment> segments)
        {
            return PlannedRoute.CalculateClimbMarkers(markers, CalculatedElevationProfile.From(plannedRoute, segments).Points);
        }

        private void BuildSegmentsAndMarkers()
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

            _segments = new List<Segment>
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

            foreach (var segment in _segments)
            {
                segment.Type = SegmentType.Segment;
                segment.Sport = SportType.Cycling;
                segment.CalculateDistances();
            }

            var reversedSegments = _segments
                .Select(ReverseSegment)
                .ToList();

            _segments.AddRange(reversedSegments);
            
            _markers = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    TrackPoint.FromTrackPoint(segment1Point2),
                    TrackPoint.FromTrackPoint(segment1Point3),
                    TrackPoint.FromTrackPoint(segment2Point1),
                    TrackPoint.FromTrackPoint(segment2Point2)
                })
                {
                    Id = "climb-1",
                    Name = "Climb 1",
                    Type = SegmentType.Climb,
                    Sport = SportType.Cycling
                },
                new(new List<TrackPoint>
                {
                    TrackPoint.FromTrackPoint(segment2Point2),
                    TrackPoint.FromTrackPoint(segment2Point1),
                    TrackPoint.FromTrackPoint(segment1Point3),
                    TrackPoint.FromTrackPoint(segment1Point2),
                })
                {
                    Id = "climb-1-rev",
                    Name = "Climb 1 reverse"
                }
            };

            foreach (var marker in _markers)
            {
                marker.Type = SegmentType.Climb;
                marker.Sport = SportType.Cycling;
                marker.CalculateDistances();
            }
        }

        private static Segment ReverseSegment(Segment input)
        {
            var reverseSegment = new Segment(
                input.Points.AsEnumerable().Reverse().Select(input1 => TrackPoint.FromTrackPoint(input1)).ToList())
            {
                Id = input.Id + "-rev",
                Name = input.Name + " reverse",
                Type = input.Type,
                Sport = input.Sport
            };
            
            reverseSegment.CalculateDistances();

            return reverseSegment;
        }
    }
}

