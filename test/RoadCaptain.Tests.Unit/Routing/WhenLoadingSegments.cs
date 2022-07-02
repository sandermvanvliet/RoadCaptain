using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenLoadingSegments
    {
        [Fact]
        public void ConvertLatLonToGameAndBack()
        {
            var trackPoint = new TrackPoint(-11.640437d, 166.946204d, 13.2d, ZwiftWorldId.Watopia);

            var gamePoint = trackPoint.ToMapCoordinate();
            var reverted = gamePoint.ToTrackPoint();

            // This is to ensure the conversion actually worked
            reverted
                .Should()
                .Be(trackPoint);

            // This is the actual test that verifies that the custom Equals method works as expected
            reverted
                .Equals(trackPoint)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GameToLatLonRepro()
        {
            var actualGame =
                JsonConvert.DeserializeObject<GameCoordinate>(
                    @"{""Latitude"":82723.9,""Longitude"":13806.059,""Altitude"":9365.3,""WorldId"":1}");

            var result = actualGame.ToTrackPoint();

            var expected = new TrackPoint(-11.64490d, 166.95293d, 9365.3, ZwiftWorldId.Watopia);

            result
                .Should()
                .Be(expected);
        }

        [Fact]
        public void ConvertGameToLatLon()
        {
            var gamePoint = new MapCoordinate(1975769797.96169d, -1697415291.43296, 0, ZwiftWorldId.Watopia);

            var expected = new TrackPoint(-11.63645, 166.97237, 0, ZwiftWorldId.Watopia);

            gamePoint.ToTrackPoint()
                .Should()
                .Be(expected);
        }

        [Fact]
        public void ConvertLatLonToGame()
        {
            var input = new TrackPoint(-11.63645, 166.97237, 0, ZwiftWorldId.Watopia);

            var expected = new GameCoordinate(1975769797.96169d, -1697415291.43296, 0, ZwiftWorldId.Watopia);
            
            input.ToMapCoordinate()
                .Should()
                .Be(expected);
        }

        [Fact]
        public void RoundtripConvert()
        {
            var input = new TrackPoint(-11.63645, 166.97237, 0, ZwiftWorldId.Watopia);

            var expected = new MapCoordinate(1975769797.96169d, -1697415291.43296, 0, ZwiftWorldId.Watopia);
            
            input.ToMapCoordinate()
                .ToTrackPoint()
                .ToMapCoordinate()
                .Should()
                .Be(expected);
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
