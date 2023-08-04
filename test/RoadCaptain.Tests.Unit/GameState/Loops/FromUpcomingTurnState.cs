using RoadCaptain.GameStates;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState.Loops
{
    public class FromUpcomingTurnState : LoopStateTransitionTestBase
    {
        [Fact]
        public void GivenOnLastLoopSegment_TotalDistanceOnRouteStateIsReset()
        {
            var result = GivenStartingState(Route).UpdatePosition(RouteSegment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .ElapsedDistance
                .Should()
                .Be(0);
        }

        private UpcomingTurnState GivenStartingState(PlannedRoute plannedRoute)
        {
            // Ensure the route has started and we're on the first route segment
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
                new List<TurnDirection> { TurnDirection.Left, TurnDirection.Right },
                1000,
                1000,
                1000);
        }
    }

    public class LoopStateTransitionTestBase : StateTransitionTestBase
    {
        protected override PlannedRoute Route { get; } = new()
        {
            RouteSegmentSequence =
            {
                new SegmentSequence(segmentId: RouteSegment1.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.LoopStart, nextSegmentId: RouteSegment2.Id, turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment2.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.Loop, nextSegmentId: RouteSegment3.Id, turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment3.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.LoopEnd)
            },
            Sport = SportType.Cycling,
            WorldId = "watopia",
            World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
            Name = "test",
            ZwiftRouteName = "zwift test route name"
        };
    }
}
