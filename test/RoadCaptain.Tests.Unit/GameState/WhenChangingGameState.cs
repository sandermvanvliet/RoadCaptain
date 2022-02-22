using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenChangingGameState
    {
        private const int ActivityId = 1234;

        [Fact]
        public void GivenNotInGameStateAndGameIsEntered_ResultingStateIsInGameStateWithActivityIdSet()
        {
            var state = new NotInGameState();

            var result = state.EnterGame(ActivityId);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .ActivityId
                .Should()
                .Be(ActivityId);
        }

        [Fact]
        public void GivenInGameStateAndGameIsExited_ResultingStateIsNotInGameState()
        {
            var state = new InGameState(ActivityId);

            var result = state.LeaveGame();

            result
                .Should()
                .BeOfType<NotInGameState>();
        }

        [Fact]
        public void GivenInGameStateAndGameIsEnteredWithSameActivityId_SameStateIsReturned()
        {
            var state = new InGameState(ActivityId);

            var result = state.EnterGame(ActivityId);

            result
                .Should()
                .BeEquivalentTo(state);
        }

        [Fact]
        public void GivenInGameStateAndGameIsEnteredWithSameActivityId_InGameStateIsReturnedWithNewActivityId()
        {
            var state = new InGameState(ActivityId);

            var result = state.EnterGame(5678);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .ActivityId
                .Should()
                .Be(5678);
        }

        [Fact]
        public void GivenInGameStateAndPositionIsUpdated_ResultingStateIsPositionedState()
        {
            var state = new InGameState(ActivityId);

            var result = state.UpdatePosition(_positionNotOnSegment, _segments, _route);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(_positionNotOnSegment);
        }

        [Fact]
        public void GivenInGameStateAndPositionIsUpdatedAndPositionIsOnSegment_ResultingStateIsOnSegmentState()
        {
            var state = new InGameState(ActivityId);

            var result = state.UpdatePosition(PositionOnSegment, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
        }

        [Fact]
        public void GivenPositionedStateAndPositionIsUpdatedWhichIsNotOnSegment_ResultingStateIsPositionedState()
        {
            var state = new PositionedState(ActivityId, _positionNotOnSegment);

            var result = state.UpdatePosition(new TrackPoint(1, 1, 1), _segments, _route);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(new TrackPoint(1, 1, 1));
        }

        [Fact]
        public void GivenPositionedStateAndPositionIsUpdatedWhichIsOnSegment_ResultingStateIsOnSegmentState()
        {
            var state = new PositionedState(ActivityId, _positionNotOnSegment);

            var result = state.UpdatePosition(PositionOnSegment, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PositionOnSegment);
        }

        [Fact]
        public void GivenOnSegmentStateAndPositionIsUpdatedWhichIsOnSameSegment_ResultingStateIsOnSegmentStateWithSameSegmentid()
        {
            var state = new OnSegmentState(ActivityId, PositionOnSegment, new Segment { Id = "segment-1" });

            var result = state.UpdatePosition(OtherOnSegment, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
        }

        [Fact]
        public void GivenOnSegmentStateAndPositionIsUpdatedWhichIsOnAnotherSegment_ResultingStateIsOnSegmentStateWithNewSegmentid()
        {
            var state = new OnSegmentState(ActivityId, PositionOnSegment, new Segment { Id = "segment-1" });

            var result = state.UpdatePosition(PositionOnAnotherSegment, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-2");
        }

        [Fact]
        public void GivenInGameStateAndPositionIsUpdatedAndPositionIsStartOfRoute_ResultingStateIsOnRouteState()
        {
            var state = new InGameState(ActivityId);

            var result = state.UpdatePosition(RoutePosition1, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("route-segment-1");
        }

        [Fact]
        public void GivenOnRouteStateAndPositionIsUpdatedAndPositionIsInSameSegment_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            var state = new OnRouteState(ActivityId, RoutePosition1, new Segment { Id = "route-segment-1" }, _route);

            var result = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("route-segment-1");
        }

        [Fact]
        public void GivenOnRouteStateAndPositionIsUpdatedAndPositionIsOnNextSegmentInRoute_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            var state = new OnRouteState(ActivityId, RoutePosition1, new Segment { Id = "route-segment-1" }, _route);

            var result = state.UpdatePosition(RoutePosition2, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("route-segment-2");
        }

        [Fact]
        public void GivenOnSegmentStateAndRouteIsInProgressAndPositionIsInStartingSegment_OnSegmentStateIsReturned()
        {
            // This test verifies that if we come across the starting segment again
            // and the route has been started before that we don't attempt to restart
            // the route.

            _route.EnteredSegment("route-segment-1");
            GameStates.GameState state = new OnRouteState(ActivityId, RoutePosition1, new Segment { Id = "route-segment-1" }, _route);
            state = state.UpdatePosition(RoutePosition3, _segments, ((OnRouteState)state).Route);

            var result = state.UpdatePosition(RoutePosition1, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("route-segment-1");
        }

        [Fact]
        public void GivenOnSegmentStateAndPositionIsUpdatedFromOneToTwo_OnSegmentStateIsReturnedWithDirectionAtoB()
        {
            var state = new OnSegmentState(ActivityId, RoutePosition1, SegmentById("route-segment-1"));

            var result = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            result
                .Should()
                .BeAssignableTo<OnSegmentState>()
                .Which
                .Direction
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        [Fact]
        public void GivenOnSegmentStateWithDirectionAtoBAndNextPositionIsSameAsLast_DirectionRemainsAtoB()
        {
            GameStates.GameState state = new OnSegmentState(ActivityId, RoutePosition1, SegmentById("route-segment-1"));
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            var result = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            result
                .Should()
                .BeAssignableTo<OnSegmentState>()
                .Which
                .Direction
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        [Fact]
        public void GivenOnRouteStateAndLeftTurnAvailable_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            GameStates.GameState state = new OnRouteState(ActivityId, RoutePosition1, new Segment { Id = "route-segment-1" }, _route);
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            var result = state.TurnCommandAvailable("TurnLeft");

            result
                .Should()
                .BeOfType<OnRouteState>();
        }

        [Fact]
        public void GivenOnRouteStateWithLeftTurnAndRightTurnAvailable_ResultingStateIsUpcomingTurnStateWithLeftAndRight()
        {
            _route.EnteredSegment("route-segment-1");
            GameStates.GameState state = new OnRouteState(ActivityId, RoutePosition1, SegmentById("route-segment-1"), _route);
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);
            state = state.TurnCommandAvailable("TurnLeft");

            var result = state.TurnCommandAvailable("TurnRight");

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .Directions
                .Should()
                .BeEquivalentTo(new List<TurnDirection> { TurnDirection.Left, TurnDirection.Right });
        }

        [Fact]
        public void GivenUpcomingTurnStateAndPositionIsOnSameSegmentBeforeTurn_ResultingStateIsUpcomingTurnState()
        {
            _route.EnteredSegment("route-segment-1");
            GameStates.GameState state = new OnRouteState(ActivityId, RoutePosition1, SegmentById("route-segment-1"), _route);
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);
            state = state.TurnCommandAvailable("TurnLeft");
            state = state.TurnCommandAvailable("TurnRight");

            var result = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>();
        }

        private Segment SegmentById(string id)
        {
            return _segments.SingleOrDefault(s => s.Id == id);
        }

        private readonly List<Segment> _segments = new()
        {
            new Segment
            {
                Id = "segment-1",
                Points =
                {
                    PositionOnSegment,
                    OtherOnSegment
                }
            },
            new Segment
            {
                Id = "segment-2",
                Points =
                {
                    PositionOnAnotherSegment
                }
            },
            new Segment
            {
                Id = "route-segment-1",
                Points =
                {
                    RoutePosition1,
                    RoutePosition1Point2
                },
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.Left, "route-segment-2"),
                    new Turn(TurnDirection.Right, "route-segment-3")
                }
            },
            new Segment
            {
                Id = "route-segment-2",
                Points =
                {
                    RoutePosition2
                }
            },
            new Segment
            {
                Id = "route-segment-3",
                Points =
                {
                    RoutePosition3
                }
            },
        };

        // Note on the positions: Keep them far away from eachother otherwise you'll
        // get some interesting test failures because they are too close together
        // and you'll end up with the wrong segment....
        private readonly TrackPoint _positionNotOnSegment = new(2, 2, 0);
        private static readonly TrackPoint PositionOnSegment = new(1, 2, 3);
        private static readonly TrackPoint OtherOnSegment = new(3, 2, 3);
        private static readonly TrackPoint PositionOnAnotherSegment = new(4, 2, 3);

        private static readonly TrackPoint RoutePosition1 = new(10, 1, 3);
        private static readonly TrackPoint RoutePosition1Point2 = new(10, 2, 3);
        private static readonly TrackPoint RoutePosition2 = new(12, 2, 3);
        private static readonly TrackPoint RoutePosition3 = new(13, 3, 3);

        private readonly PlannedRoute _route = new()
        {
            RouteSegmentSequence =
            {
                new SegmentSequence { SegmentId = "route-segment-1", NextSegmentId = "route-segment-2", TurnToNextSegment = TurnDirection.Left },
                new SegmentSequence { SegmentId = "route-segment-2", NextSegmentId = "route-segment-3", TurnToNextSegment = TurnDirection.Right },
                new SegmentSequence { SegmentId = "route-segment-3" },
            }
        };
    }
}