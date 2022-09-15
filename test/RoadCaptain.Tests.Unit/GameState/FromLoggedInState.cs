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
        public void EnteringGameWithRiderAndActivityId_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().EnterGame(1, 2);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
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

        private LoggedInState GivenStartingState()
        {
            return new LoggedInState();
        }
    }
}
