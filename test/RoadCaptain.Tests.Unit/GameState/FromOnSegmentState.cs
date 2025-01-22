// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromOnSegmentState : StateTransitionTestBase
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
        public void GivenNextPositionOnSegment_ResultIsOnSegmentStateWithDirectionAtoB()
        {
            var result = GivenStartingState()
                .UpdatePosition(Segment1Point1, Segments, Route)
                .UpdatePosition(Segment1Point2, Segments, Route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .Direction
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentStateForSegmentOne()
        {
            var result = GivenStartingState()
                .UpdatePosition(Segment2Point1, Segments, Route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be(Segment2.Id);
        }

        [Fact]
        public void GivenPositionOnSegmentAndFirstSegmentOfRoute_ResultIsOnSegmentState()
        {
            var result = GivenStartingState()
                .UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegmentAndFirstSegmentOfRouteButInWrongDirection_ResultIsLostRouteLockState()
        {
            GameStates.GameState state = GivenStartingState();
            state = state.UpdatePosition(RouteSegment1Point2, Segments, Route);

            var result = state
                .UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegmentAndFirstSegmentOfRouteRouteIsStarted()
        {
            GameStates.GameState state = GivenStartingState();
            state = state.UpdatePosition(RouteSegment1Point1, Segments, Route);

            state = state.UpdatePosition(RouteSegment1Point2, Segments, Route);

            Route
                .HasStarted
                .Should()
                .BeTrue();
            
            Route
                .CurrentSegmentId
                .Should()
                .Be(RouteSegment1.Id);
        }

        [Fact]
        public void GivenRouteHasStartedAndPositionOnSegmentAndFirstSegmentOfRoute_InvalidStateTransitionExceptionIsThrown()
        {
            Route.EnteredSegment(RouteSegment1.Id);
            
            var action = () => GivenStartingState().UpdatePosition(RouteSegment1Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>("from a started route you can only go to OnRouteState from LostRouteLockState or UpcomingTurnState");
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId_SameStateIsReturned()
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

        private OnSegmentState GivenStartingState()
        {
            return new OnSegmentState(1, 2, Segment1Point1, Segment1, SegmentDirection.AtoB, 0, 0, 0);
        }
    }
}
