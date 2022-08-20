using System;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromCompletedRouteState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_ResultIsCompletedRouteStateWithNewPosition()
        {
            var result = GivenStartingState()
                .UpdatePosition(PointNotOnAnySegment, Segments, Route);

            result
                .Should()
                .BeOfType<CompletedRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PointNotOnAnySegment);
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsCompletedRouteStateWithNewPosition()
        {
            var result = GivenStartingState().UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<CompletedRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(Segment1Point1);
        }

        [Fact]
        public void GivenPositionOnRouteSegment_ResultIsCompletedRouteStateWithNewPosition()
        {
            var result = GivenStartingState()
                .UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<CompletedRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RouteSegment1Point1);
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId_InvalidStateTransitionExceptionIsThrown()
        {
            Action action = () => GivenStartingState().EnterGame(1, 3);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void EnteringGameWithSameRiderAndDifferentActivityId_InvalidStateTransitionExceptionIsThrown()
        {
            Action action = () => GivenStartingState().EnterGame(1, 3);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void EnteringGameWithDifferentRiderAndSameActivityId_InvalidStateTransitionExceptionIsThrown()
        {
            Action action = () => GivenStartingState().EnterGame(2, 2);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
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
        public void TurnCommandAvailable_SameStateIsReturned()
        {
            var startingState = GivenStartingState();
            var result = startingState.TurnCommandAvailable("left");

            result
                .Should()
                .Be(startingState);
        }

        private CompletedRouteState GivenStartingState()
        {
            Route.Complete();

            return new CompletedRouteState(1, 2, RouteSegment3Point3, Route);
        }
    }
}
