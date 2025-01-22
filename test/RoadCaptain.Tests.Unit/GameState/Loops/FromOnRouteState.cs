// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.GameStates;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState.Loops
{
    public class FromOnRouteState : LoopStateTransitionTestBase
    {
        [Fact]
        public void GivenOnRouteAndSegmentIsLoopStart_ResultIsOnLoopState()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnLoopState>();
        }

        [Fact]
        public void
            GivenOnLoopEndSegmentOfRouteAndOnLoopTwoOfFive_WhenTurnsBecomeAvailable_UpcomingTurnStateIsReturned()
        {
            var result = GivenStartingState(Route)
                .UpdatePosition(RouteSegment1Point1, Segments, Route)
                .UpdatePosition(RouteSegment2Point1, Segments, Route)
                .UpdatePosition(RouteSegment3Point3, Segments, Route)
                .TurnCommandAvailable("turnleft")
                .TurnCommandAvailable("gostraight");

            result
                .Should()
                .BeOfType<UpcomingTurnState>();
        }

        private OnRouteState GivenStartingState(PlannedRoute plannedRoute)
        {
            plannedRoute.EnteredSegment(RouteSegment0.Id);

            return new OnRouteState(
                1,
                2,
                RouteSegment0Point3,
                RouteSegment0,
                plannedRoute,
                SegmentDirection.AtoB,
                0,
                0,
                0);
        }
    }
}

