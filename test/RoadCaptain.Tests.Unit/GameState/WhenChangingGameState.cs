using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameState;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenChangingGameState
    {
        [Fact]
        public void GivenNotInGameStateAndPositionIsUpdated_ResultingStateIsNotInGameState()
        {
            var state = new NotInGameState();

            var result = state.UpdatePosition(new TrackPoint(0, 0, 0), _segments);

            result
                .Should()
                .BeOfType<NotInGameState>();
        }

        [Fact]
        public void GivenNotInGameStateAndGameIsEntered_ResultingStateIsInGameStateWithActivityIdSet()
        {
            var state = new NotInGameState();

            var result = state.EnterGame(1234);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .ActivityId
                .Should()
                .Be(1234);
        }

        [Fact]
        public void GivenNotInGameStateAndSegmentIsEntered_ResultingStateIsNotInGameState()
        {
            var state = new NotInGameState();

            var result = state.EnterSegment();

            result
                .Should()
                .BeOfType<NotInGameState>();
        }

        [Fact]
        public void GivenNotInGameStateAndGameIsExited_ResultingStateIsNotInGameState()
        {
            var state = new NotInGameState();

            var result = state.LeaveGame();

            result
                .Should()
                .BeOfType<NotInGameState>();
        }

        [Fact]
        public void GivenInGameStateAndGameIsExited_ResultingStateIsNotInGameState()
        {
            var state = new InGameState(1234);

            var result = state.LeaveGame();

            result
                .Should()
                .BeOfType<NotInGameState>();
        }

        [Fact]
        public void GivenInGameStateAndPositionIsUpdated_ResultingStateIsPositionedState()
        {
            var state = new InGameState(1234);

            var result = state.UpdatePosition(_positionNotOnSegment, _segments);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(_positionNotOnSegment);
        }

        [Fact]
        public void GivenInGameStateAndPositionIsUpdatedAndPositionIsOnSegment_ResultingStateIsOnSegmentState()
        {
            var state = new InGameState(1234);

            var result = state.UpdatePosition(PositionOnSegment, _segments);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
        }

        [Fact]
        public void GivenPositionedStateAndPositionIsUpdatedWhichIsNotOnSegment_ResultingStateIsPositionedState()
        {
            var state = new PositionedState(1234, _positionNotOnSegment);

            var result = state.UpdatePosition(new TrackPoint(1, 1, 1), _segments);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(new TrackPoint(1, 1, 1));
        }

        [Fact]
        public void GivenPositionedStateAndPositionIsUpdatedWhichIsOnSegment_ResultingStateIsOnSegmentState()
        {
            var state = new PositionedState(1234, _positionNotOnSegment);

            var result = state.UpdatePosition(PositionOnSegment, _segments);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PositionOnSegment);
        }

        [Fact]
        public void GivenOnSegmentStateAndPositionIsUpdatedWhichIsOnSameSegment_ResultingStateIsOnSegmentStateWithSameSegmentid()
        {
            var state = new OnSegmentState(1234, PositionOnSegment, new Segment { Id = "segment-1" });

            var result = state.UpdatePosition(OtherOnSegment, _segments);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
        }

        [Fact]
        public void GivenOnSegmentStateAndPositionIsUpdatedWhichIsOnAnotherSegment_ResultingStateIsOnSegmentStateWithNewSegmentid()
        {
            var state = new OnSegmentState(1234, PositionOnSegment, new Segment { Id = "segment-1" });

            var result = state.UpdatePosition(PositionOnAnotherSegment, _segments);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-2");
        }

        private readonly List<Segment> _segments = new()
        {
            new Segment
            {
                Id = "segment-1",
                Points =
                {
                    PositionOnSegment,
                    OtherOnSegment
                }
            },
            new Segment
            {
                Id = "segment-2",
                Points =
                {
                    PositionOnAnotherSegment
                }
            }
        };

        private readonly TrackPoint _positionNotOnSegment = new(2, 2 , 0);
        private static readonly TrackPoint PositionOnSegment = new(1, 2, 3);
        private static readonly TrackPoint OtherOnSegment = new(3, 2, 3);
        private static readonly TrackPoint PositionOnAnotherSegment = new(4, 2, 3);
    }
}