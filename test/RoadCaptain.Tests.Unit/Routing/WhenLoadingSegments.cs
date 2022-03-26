using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using RoadCaptain.Adapters;
using RoadCaptain.SegmentBuilder;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenLoadingSegments
    {
        [Fact]
        public void ConvertLatLonToGameAndBack()
        {
            var trackPoint = new TrackPoint(-11.640437d, 166.946204d, 13.2d);

            var gamePoint = TrackPoint.LatLongToGame(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude);
            var reverted = TrackPoint.FromGameLocation(gamePoint.Latitude, gamePoint.Longitude, gamePoint.Altitude);

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

            var reverted = TrackPoint.FromGameLocation(gamePoint.Latitude, gamePoint.Longitude, gamePoint.Altitude);

            reverted
                .CoordinatesDecimal
                .Should()
                .Be("S11.63645° E166.97237°");
        }

        [Fact]
        public void BoundingBoxesCalculated()
        {
            var segmentStore = new SegmentStore();

            var segments = segmentStore.LoadSegments();

            segments
                .All(s => s.BoundingBox != null)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void AllPointsOnEachSegmentAreWithinItsBoundingBox()
        {
            var segmentStore = new SegmentStore();

            var segments = segmentStore.LoadSegments();

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
    }
}
