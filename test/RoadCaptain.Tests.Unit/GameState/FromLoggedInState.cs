using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromLoggedInState : StateTransitionTestBase
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
            var startingState = GivenStartingState();

            var result = startingState.EnterGame(1, 2);

            result
                .Should()
                .BeOfType<InGameState>();
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

        private LoggedInState GivenStartingState()
        {
            return new LoggedInState("derp");
        }
    }
}
