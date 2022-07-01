using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using RoadCaptain.Adapters;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenLoadingSegments
    {
        [Fact]
        public void ConvertLatLonToGameAndBack()
        {
            var trackPoint = new TrackPoint(-11.640437d, 166.946204d, 13.2d);

            var gamePoint = TrackPoint.ToGameCoordinate(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude, ZwiftWorldId.Watopia);
            var reverted = TrackPoint.FromGameLocation(gamePoint.X, gamePoint.Y, gamePoint.Altitude, ZwiftWorldId.Watopia);

            reverted
                .Equals(trackPoint)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ConvertGameToLatLon()
        {
            var gameLat = 93536.016d;
            var gameLon = 212496.77d;
            var gamePoint = new TrackPoint(gameLat, gameLon, 0);

            var reverted = TrackPoint.FromGameLocation(gamePoint.Latitude, gamePoint.Longitude, gamePoint.Altitude, ZwiftWorldId.Watopia);

            reverted
                .CoordinatesDecimal
                .Should()
                .Be("S11.63645° E166.97237°");
        }

        [Fact]
        public void BoundingBoxesCalculated()
        {
            var segmentStore = new SegmentStore();

            var segments = segmentStore.LoadSegments(new World { Id = "watopia", Name = "Watopia" }, SportType.Both);

            segments
                .All(s => s.BoundingBox != null)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void AllPointsOnEachSegmentAreWithinItsBoundingBox()
        {
            var segmentStore = new SegmentStore();

            var segments = segmentStore.LoadSegments(new World { Id = "watopia", Name = "Watopia" }, SportType.Both);

            foreach (var segment in segments)
            {
                foreach (var point in segment.Points)
                {
                    if (!segment.BoundingBox.IsIn(point))
                    {
                        throw new AssertionFailedException(
                            $"Expected point {point.Latitude:0.00000}, {point.Longitude:0.00000} to be inside bounding box but it was not");
                    }
                }
            }
        }

        [Fact]
        public void GivenWorldWithoutSegments_NoSegmentsAreReturned()
        {
            var segmentStore = new SegmentStore();

            var segments = segmentStore.LoadSegments(new World { Id = "test", Name = "Test" }, SportType.Both);

            segments
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenWatopiaSegmentsAndSportIsBoth_AllSegmentsAreReturned()
        {
            var segmentStore = new SegmentStore();

            var allSegments = segmentStore.LoadSegments(new World { Id = "watopia", Name = "Watopia" }, SportType.Both);

            var bikeSegments = segmentStore.LoadSegments(new World { Id = "watopia", Name = "Watopia" }, SportType.Cycling);
            var runSegments = segmentStore.LoadSegments(new World { Id = "watopia", Name = "Watopia" }, SportType.Running);

            bikeSegments
                .Should()
                .HaveCount(allSegments.Count - runSegments.Count(r => r.Sport == SportType.Running), "there are less cycling segments than running segments");

            runSegments
                .Should()
                .HaveCount(allSegments.Count, "there are more running segments than cycling segments");
        }
    }
}
