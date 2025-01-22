// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.GameStates;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState.Loops
{
    public class FromUpcomingTurnState : LoopStateTransitionTestBase
    {
        [Fact]
        public void GivenOnLastLoopSegmentAndNextPositionIsOnLeadOutSegment_OnRouteStateIsReturned()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment0Point3, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>();
        }
        [Fact]
        public void GivenOnLastLoopSegmentAndLoopCountIsOneOfFourAndNextPositionIsOnLeadOutSegment_LostRouteLockStateIsReturned()
        {
            Route.NumberOfLoops = 4;
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment0Point3, Segments, Route);

            result
                .Should()
                .BeOfType<LostRouteLockState>();
        }

        private UpcomingTurnState GivenStartingState(PlannedRoute plannedRoute)
        {
            plannedRoute.EnteredSegment(RouteSegment0.Id);
            plannedRoute.EnteredSegment(RouteSegment1.Id);
            plannedRoute.EnteredSegment(RouteSegment2.Id);
            plannedRoute.EnteredSegment(RouteSegment3.Id);

            return new UpcomingTurnState(
                1,
                2,
                RouteSegment3Point3,
                RouteSegment3,
                plannedRoute,
                SegmentDirection.AtoB,
                new List<TurnDirection> { TurnDirection.GoStraight, TurnDirection.Right },
                1000,
                1000,
                1000);
        }
    }
}

