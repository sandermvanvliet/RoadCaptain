// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Avalonia;
using FluentAssertions;
using RoadCaptain.App.Shared.Controls;
using Xunit;

namespace RoadCaptain.App.Shared.Tests.Unit.Controls
{
    public class WhenCalculatingRenderParameters
    {
        private static readonly Rect ViewPortBounds = new(0, 0, 200, 100);
        private List<Segment> _markers = null!;
        private List<Segment> _segments = null!;

        public WhenCalculatingRenderParameters()
        {
            BuildSegmentsAndMarkers();
        }

        [Fact]
        public void GivenRenderModeAll_TotalPlotBoundsAreEqualToViewPortBounds()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.All);

            renderParameters.TotalPlotBounds.Width.Should().Be(ViewPortBounds.Width);
        }

        [Fact]
        public void GivenRenderModeAll_TotalDistanceIsEightHundredMeters_MetersPerPixelIsFour()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.All);

            renderParameters.MetersPerPixel.Should().Be(4);
        }

        [Fact]
        public void GivenRenderModeAllSegment_RiderNotOnSegment_TotalPlotBoundsAreEqualToViewPortBounds()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.AllSegment, _segments[0].Points[0]);

            renderParameters.TotalPlotBounds.Width.Should().Be(ViewPortBounds.Width);
        }

        [Fact]
        public void GivenRenderModeAllSegment_RiderNotOnSegment_MetersPerPixelIsFour()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.AllSegment, _segments[0].Points[0]);

            renderParameters.MetersPerPixel.Should().Be(4);
        }

        [Fact]
        public void GivenRenderModeAllSegment_RiderIsOnSegment_MetersPerPixelIsTwo()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.AllSegment, _segments[1].Points[0]);

            renderParameters.MetersPerPixel.Should().Be(1.6);
        }

        [Fact]
        public void GivenRenderModeAllSegment_RiderIsOnSegment_TotalPlotBoundsWidthShouldBeFiveHundred()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.AllSegment, _segments[1].Points[0]);

            renderParameters.TotalPlotBounds.Width.Should().Be(500);
        }

        [Fact]
        public void GivenRenderModeAllSegment_RiderIsOnSegment_TranslateXShouldBeMinusFifty()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.AllSegment, _segments[1].Points[0]);

            renderParameters.TranslateX.Should().Be(-56);
        }

        [Fact]
        public void GivenRenderModeMoving_MetersPerPixelIsTwoPointFive()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.Moving);

            renderParameters.MetersPerPixel.Should().Be(2.5); // The moving window is 500m
        }

        [Fact]
        public void GivenRenderModeMoving_TotalPlotBoundsWidthShouldBe()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.Moving);

            renderParameters.TotalPlotBounds.Width.Should().Be(320);
        }

        [Fact]
        public void GivenRenderModeMoving_RiderPositionOnFourHundredMeters_TranslateXIsMinusHundredTwenty()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.Moving, _segments[1].Points[2]);

            renderParameters.TranslateX.Should().Be(-120);
        }

        [Fact]
        public void GivenRenderModeMoving_RiderPositionOnZeroMeters_TranslateXIsMinusHundredTwenty()
        {
            var renderParameters = CalculateRenderParameters(RenderMode.Moving, _segments[0].Points[0]);

            renderParameters.TranslateX.Should().Be(0);
        }

        private RenderParameters CalculateRenderParameters(RenderMode renderMode, TrackPoint? riderPosition = null)
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia, Name = "Watopia" },
                WorldId = "watopia",
                Sport = SportType.Cycling,
                ZwiftRouteName = "Test Zwift Route"
            };
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-1", SegmentDirection.AtoB,
                SegmentSequenceType.Regular));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-2", SegmentDirection.AtoB,
                SegmentSequenceType.Regular));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-3", SegmentDirection.AtoB,
                SegmentSequenceType.Regular));
            var elevationProfile = CalculatedElevationProfile.From(plannedRoute, _segments);
            
            return RenderParameters.From(
                renderMode, 
                ViewPortBounds, 
                elevationProfile, 
                riderPosition ?? TrackPoint.Unknown,
                _markers);
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
                    Name = "Segment 1",
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
                },
                new(new List<TrackPoint>
                {
                    segment2Point1,
                    segment2Point2,
                    segment2Point3
                })
                {
                    Id = "segment-2",
                    Name = "Segment 2",
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
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
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
                },
            };

            foreach (var segment in _segments)
            {
                var index = 0;
                foreach (var trackPoint in segment.Points)
                {
                    trackPoint.Segment = segment;
                    trackPoint.Index = index++;
                }

                segment.CalculateDistances();
            }

            var climb1Point1 = segment1Point2.Clone();
            var climb1Point2 = segment1Point3.Clone();
            var climb1Point3 = segment2Point1.Clone();
            var climb1Point4 = segment2Point2.Clone();

            _markers = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    climb1Point1,
                    climb1Point2,
                    climb1Point3,
                    climb1Point4
                })
                {
                    Id = "climb-1",
                    Name = "Climb 1",
                    Type = SegmentType.Climb,
                    Sport = SportType.Cycling
                }
            };

            foreach (var segment in _markers)
            {
                var index = 0;
                foreach (var trackPoint in segment.Points)
                {
                    trackPoint.Segment = segment;
                    trackPoint.Index = index++;
                }

                segment.CalculateDistances();
            }
        }
    }
}

