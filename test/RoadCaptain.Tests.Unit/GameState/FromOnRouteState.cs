using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromOnRouteState : StateTransitionTestBase
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
        public void GivenRouteNotStartedAndPositionOnRouteSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState();
            // Do this here because GivenStartingState() starts the route as well
            Route.Reset();

            var action = () => startingState.UpdatePosition(RouteSegment1Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>("you can only be on-route if it's been started");
        }

        [Fact]
        public void GivenPositionOnSegmentNotOnRoute_ResultIsLostRouteLockState()
        {
            var result = GivenStartingState().UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        [Fact]
        public void GivenNextPositionOnRouteSegment_ResultIsOnRouteState()
        {
            var result = GivenStartingState().UpdatePosition(RouteSegment1Point2, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RouteSegment1Point2);
        }

        [Fact]
        public void GivenPositionOnNextSegmentOfRoute_ResultIsOnRouteState()
        {
            var result = GivenStartingState().UpdatePosition(RouteSegment2Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be(RouteSegment2.Id);
        }

        [Fact]
        public void GivenPositionOnNextSegmentOfRoute_CurrentSegmentOnRouteIsChangedToNextSegment()
        {
            var result = GivenStartingState().UpdatePosition(RouteSegment2Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .Route
                .CurrentSegmentId
                .Should()
                .Be(RouteSegment2.Id);
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionNextOnLastSegment_ResultIsOnRouteState()
        {
            var startingState = GivenStartingState();
            Route.EnteredSegment(RouteSegment2.Id);
            Route.EnteredSegment(RouteSegment3.Id);

            var result = startingState.UpdatePosition(RouteSegment3Point3, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>();
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionOnNextConnectingSegment_ResultIsCompletedRouteState()
        {
            var startingState = GivenStartingState();
            Route.EnteredSegment(RouteSegment2.Id);
            Route.EnteredSegment(RouteSegment3.Id);

            var result = startingState.UpdatePosition(Segment2Point1, Segments, Route);

            result
                .Should()
                .BeOfType<CompletedRouteState>();
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionOnNotConnectingSegment_ResultIsLostRouteLockState()
        {
            var startingState = GivenStartingState();
            Route.EnteredSegment(RouteSegment2.Id);
            Route.EnteredSegment(RouteSegment3.Id);

            var result = startingState.UpdatePosition(Segment3Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        [Fact]
        public void GivenRouteHasCompletedAndPositionIsOnLastSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState();
            Route.EnteredSegment(RouteSegment2.Id);
            Route.EnteredSegment(RouteSegment3.Id);
            Route.Complete();

            var action = () => startingState.UpdatePosition(Segment3Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().EnterGame(1, 2);

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
        public void TurnCommandAvailable_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().TurnCommandAvailable("left");

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        private OnRouteState GivenStartingState()
        {
            // Ensure the route has started and we're on the first route segment
            Route.EnteredSegment(RouteSegment1.Id);

            return new OnRouteState(
                1,
                2,
                RouteSegment1Point1,
                RouteSegment1,
                Route,
                SegmentDirection.AtoB,
                0,
                0,
                0);
        }
    }
}