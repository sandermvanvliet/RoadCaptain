// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.GameStates;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState.Loops
{
    public class FromOnLoopState : LoopStateTransitionTestBase
    {
        [Fact]
        public void GivenOnLoopAndPositionIsOnLeadOut_ResultIsOnRouteState()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment0Point3, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>();
        }

        private OnLoopState GivenStartingState(PlannedRoute plannedRoute)
        {
            plannedRoute.EnteredSegment(RouteSegment0.Id);
            plannedRoute.EnteredSegment(RouteSegment1.Id);
            plannedRoute.EnteredSegment(RouteSegment2.Id);
            plannedRoute.EnteredSegment(RouteSegment3.Id);
            
            return new OnLoopState(
                1,
                2,
                RouteSegment3Point3,
                RouteSegment3,
                plannedRoute,
                SegmentDirection.AtoB,
                0,
                0,
                0,
                1);
        }
    }
}

