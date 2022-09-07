using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromPositionedState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_ResultIsPositionedState()
        {
            var result = GivenStartingState()
                .UpdatePosition(PointNotOnAnySegment, Segments, Route);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PointNotOnAnySegment);
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentState()
        {
            var result = GivenStartingState()
                .UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentStateForSegmentOne()
        {
            var result = GivenStartingState()
                .UpdatePosition(Segment1Point1, Segments, Route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be(Segment1.Id);
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId__SameStateIsReturned()
        {
            var startingState = GivenStartingState();
            var result = startingState.EnterGame(1, 2);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void LeavingGame_ConnectedToZwiftStateIsReturned()
        {
            var result = GivenStartingState().LeaveGame();

            result
                .Should()
                .BeOfType<ConnectedToZwiftState>();
        }

        [Fact]
        public void TurnCommandAvailable_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().TurnCommandAvailable("left");

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        private PositionedState GivenStartingState()
        {
            return new PositionedState(1, 2, PointNotOnAnySegment);
        }
    }
}