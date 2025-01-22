// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromUpcomingTurnState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_ResultIsPositionedState()
        {
            var result = GivenStartingState(Route)
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
            var startingState = GivenStartingState(Route);
            // Do this here because GivenStartingState() starts the route as well
            Route.Reset();

            var action = () => startingState.UpdatePosition(RouteSegment1Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>("you can only be on-route if it's been started");
        }

        [Fact]
        public void GivenRouteCompletedAndPositionOnRouteSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState(Route);
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
            var result = GivenStartingState(Route).UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        [Fact]
        public void GivenNextPositionOnRouteSegmentInSameDirection_ResultIsUpcomingTurnState()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment1Point2, Segments, Route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RouteSegment1Point2);
        }

        [Fact]
        public void GivenNextPositionOnRouteSegmentInReverseDirection_ResultIsLostRouteLockState()
        {
            var stage1 = GivenStartingState(Route).UpdatePosition(RouteSegment1Point2, Segments, Route);

            var result = stage1.UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RouteSegment1Point1);
        }

        [Fact]
        public void GivenNextPositionOnRouteSegment_ElapsedDistanceIsIncreased()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment1Point2, Segments, Route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .ElapsedDistance
                .Should()
                .NotBe(0);
        }

        [Fact]
        public void GivenNextPositionOnRouteSegment_ElapsedAscentIsIncreased()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment1Point2, Segments, Route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .ElapsedAscent
                .Should()
                .NotBe(0);
        }

        [Fact]
        public void GivenNextPositionOnRouteSegment_ElapsedDescentIsIncreased()
        {
            var result = GivenStartingState(Route)
                .UpdatePosition(RouteSegment1Point2, Segments, Route)
                .UpdatePosition(RouteSegment1Point3, Segments, Route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .ElapsedDescent
                .Should()
                .NotBe(0);
        }

        [Fact]
        public void GivenPositionOnNextSegmentOfRoute_ResultIsOnRouteState()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment2Point1, Segments, Route);

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
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment2Point1, Segments, Route);

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
        public void GivenOnLastSegmentOfRouteAndPositionNextOnLastSegmentInSameDirection_ResultIsUpcomingTurnState()
        {
            GameStates.GameState state = GivenStartingState(Route);
            state = state.UpdatePosition(RouteSegment2Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point2, Segments, Route); // Needed to determine direction
            state = state.TurnCommandAvailable("turnleft");
            state = state.TurnCommandAvailable("gostraight");

            var result = state.UpdatePosition(RouteSegment3Point3, Segments, Route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>();
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionNextOnLastSegmentInOppositeDirection_ResultIsLostRouteLockState()
        {
            GameStates.GameState state = GivenStartingState(Route);
            state = state.UpdatePosition(RouteSegment2Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point2, Segments, Route); // Needed to determine direction
            state = state.TurnCommandAvailable("turnleft");
            state = state.TurnCommandAvailable("gostraight");

            var result = state.UpdatePosition(RouteSegment3Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionOnNextConnectingSegment_ResultIsCompletedRouteState()
        {
            GameStates.GameState state = GivenStartingState(Route);
            state = state.UpdatePosition(RouteSegment2Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point2, Segments, Route); // Needed to determine direction
            state = state.TurnCommandAvailable("turnleft");
            state = state.TurnCommandAvailable("gostraight");

            var result = state.UpdatePosition(Segment2Point1, Segments, Route);

            result
                .Should()
                .BeOfType<CompletedRouteState>();
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndPositionOnNotConnectingSegment_ResultIsLostRouteLockState()
        {
            GameStates.GameState state = GivenStartingState(Route);
            state = state.UpdatePosition(RouteSegment2Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point1, Segments, Route);
            state = state.UpdatePosition(RouteSegment3Point2, Segments, Route); // Needed to determine direction
            state = state.TurnCommandAvailable("turnleft");
            state = state.TurnCommandAvailable("gostraight");

            var result = state.UpdatePosition(Segment3Point1, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        [Fact]
        public void GivenRouteHasCompletedAndPositionIsOnLastSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState(Route);
            Route.EnteredSegment(RouteSegment2.Id);
            Route.EnteredSegment(RouteSegment3.Id);
            Route.Complete();

            var action = () => startingState.UpdatePosition(Segment3Point1, Segments, Route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId_SameStateIsReturned()
        {
            var startingState = GivenStartingState(Route);
            var result = startingState.EnterGame(1, 2);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void LeavingGame_ConnectedToZwiftStateIsReturned()
        {
            var result = GivenStartingState(Route).LeaveGame();

            result
                .Should()
                .BeOfType<ConnectedToZwiftState>();
        }

        [Fact]
        public void GivenLeftTurnAvailableAndCurrentDirectionIsUnknown_SameStateIsReturned()
        {
            var plannedRoute = GivenPlannedRoute();
            var nextState = GivenStartingState(plannedRoute)
                .UpdatePosition(RouteSegment2Point1, Segments, plannedRoute);

            var result = nextState.TurnCommandAvailable("turnleft");

            result
                .Should()
                .Be(nextState);
        }

        [Fact]
        public void GivenLeftTurnAvailableAndWasAlreadyAvailable_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState(Route);
            var result = startingState.TurnCommandAvailable("turnleft");

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenTurnCommandIsNone_SameStateIsReturned()
        {
            var startingState = GivenStartingState(Route);
            var result = startingState.TurnCommandAvailable("whatever");

            result
                .Should()
                .Be(startingState);
        }

        private UpcomingTurnState GivenStartingState(PlannedRoute plannedRoute)
        {
            // Ensure the route has started and we're on the first route segment
            plannedRoute.EnteredSegment(RouteSegment1.Id);

            return new UpcomingTurnState(
                1,
                2,
                RouteSegment1Point1,
                RouteSegment1,
                plannedRoute,
                SegmentDirection.AtoB,
                new List<TurnDirection> { TurnDirection.Left, TurnDirection.Right },
                0,
                0,
                0);
        }
    }
}
