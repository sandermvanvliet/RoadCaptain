using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromConnectedToZwiftState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().UpdatePosition(PointNotOnAnySegment, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void GivenPositionOnSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().UpdatePosition(Segment1Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void EnteringGameWithRiderAndActivityId_ResultIsInGameState()
        {
            var result = GivenStartingState().EnterGame(1, 2);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .RiderId
                .Should()
                .Be(1);
        }

        [Fact]
        public void LeavingGame_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().LeaveGame();

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void TurnCommandAvailable_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().TurnCommandAvailable("left");

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        private ConnectedToZwiftState GivenStartingState()
        {
            return new ConnectedToZwiftState();
        }
    }
}
