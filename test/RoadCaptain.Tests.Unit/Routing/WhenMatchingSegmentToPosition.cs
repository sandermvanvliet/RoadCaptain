using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenMatchingSegmentToPosition
    {
        private readonly Segment _segment;

        public WhenMatchingSegmentToPosition()
        {
            _segment = new Segment
            {
                Id = "S1",
                NextSegmentsNodeA = {
                            new(TurnDirection.GoStraight, "S2"),
                            new(TurnDirection.Left, "S3"),
                },
                NextSegmentsNodeB = {
                            new(TurnDirection.GoStraight, "S4"),
                }
            };
            _segment.Points.AddRange(
                new List<TrackPoint>
                {
                    new(0, 0, 0),
                    new(0, 0.1m, 0),
                    new(0, 0.2m, 0),
                    new(0, 0.3m, 0),
                    new(0, 0.4m, 0),
                    new(0, 0.5m, 0),
                    new(0, 0.6m, 0),
                });
        }

        [Fact]
        public void GivenPositionOutsideSegment_NoMatchIsReturned()
        {
            var position = new TrackPoint(2, 2, 0);

            _segment
                .Contains(position)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GivenPositionOnSegment_MatchIsReturned()
        {
            var position = new TrackPoint(0, 0.5m, 0);

            _segment
                .Contains(position)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenPointsInSequenceAtoB_DirectionShouldBeAtoB()
        {
            var first = new TrackPoint(0, 0.5m, 0);
            var second = new TrackPoint(0, 0.6m, 0);

            _segment
                .DirectionOf(first, second)
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        [Fact]
        public void GivenPointsInSequenceBtoA_DirectionShouldBeBtoA()
        {
            var first = new TrackPoint(0, 0.6m, 0);
            var second = new TrackPoint(0, 0.5m, 0);

            _segment
                .DirectionOf(first, second)
                .Should()
                .Be(SegmentDirection.BtoA);
        }

        [Fact]
        public void GivenSegmentWithSingleSegmentAfterB_StraightTurnWithNextSegmentIsReturned()
        {
            _segment
                .NextSegments(SegmentDirection.AtoB)
                .Select(kv => kv.Direction)
                .Should()
                .BeEquivalentTo(new[] { TurnDirection.GoStraight });
        }

        [Fact]
        public void GivenSegmentWithTwoSegmentAfterA_StraightOnAndLeftAreReturned()
        {
            _segment
                .NextSegments(SegmentDirection.BtoA)
                .Select(kv => kv.Direction)
                .Should()
                .BeEquivalentTo(new[] { TurnDirection.Left, TurnDirection.GoStraight });
        }
    }
}