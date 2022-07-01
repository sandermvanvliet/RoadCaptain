using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenChangingGameState
    {
        private const uint RiderId = 56789;
        private const int ActivityId = 1234;

        [Fact]
        public void GivenNotInGameStateAndGameIsEntered_ResultingStateIsInGameStateWithActivityIdSet()
        {
            var state = new ConnectedToZwiftState();

            var result = state.EnterGame(RiderId, ActivityId);

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
            var state = new InGameState(RiderId, ActivityId);

            var result = state.LeaveGame();

            result
                .Should()
                .BeOfType<ConnectedToZwiftState>();
        }

        [Fact]
        public void GivenInGameStateAndGameIsEnteredWithSameActivityId_SameStateIsReturned()
        {
            var state = new InGameState(RiderId, ActivityId);

            var result = state.EnterGame(RiderId, ActivityId);

            result
                .Should()
                .BeEquivalentTo(state);
        }

        [Fact]
        public void GivenInGameStateAndGameIsEnteredWithDifferentActivityId_InGameStateIsReturnedWithNewActivityId()
        {
            var state = new InGameState(RiderId, ActivityId);

            var result = state.EnterGame(RiderId, 5678);

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
            var state = new InGameState(RiderId, ActivityId);

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
            var state = new InGameState(RiderId, ActivityId);

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
            var state = new PositionedState(RiderId, ActivityId, _positionNotOnSegment);

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
            var state = new PositionedState(RiderId, ActivityId, _positionNotOnSegment);

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
            var state = new OnSegmentState(RiderId, ActivityId, PositionOnSegment, SegmentById("segment-1"));

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
            var state = new OnSegmentState(RiderId, ActivityId, PositionOnSegment, SegmentById("segment-1"));

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
            var state = new InGameState(RiderId, ActivityId);

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
            var state = new OnRouteState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"), _route);

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
            var state = new OnRouteState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"), _route);

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
        public void GivenOnSegmentStateAndPositionIsUpdatedAndPositionIsOnNextSegmentInRoute_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            var state = new OnSegmentState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"));

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
        public void GivenOnSegmentStateAndPositionIsUpdatedFromOneToTwo_OnSegmentStateIsReturnedWithDirectionAtoB()
        {
            var state = new OnSegmentState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"));

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
            GameStates.GameState state = new OnSegmentState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"));
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
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition1, SegmentById("route-segment-1"), _route);
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);

            var result = state.TurnCommandAvailable("TurnLeft");

            result
                .Should()
                .BeOfType<OnRouteState>();
        }

        [Fact]
        public void GivenOnSegmentStateAndRouteHasStarted_ResultingStateIsOnRoute()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");

            var state = new OnSegmentState(RiderId, ActivityId, RoutePosition2, SegmentById("segment-1"));

            var result = state.UpdatePosition(RoutePosition2, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .Route
                .CurrentSegmentId
                .Should()
                .Be("route-segment-2");
        }

        [Fact]
        public void
            GivenUpcomingTurnStateAndPositionIsUpdatedOnRouteBeforeTurn_ResultingStateIsUpcomingTurnWithNewPosition()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition2, SegmentById("route-segment-2"), _route);
            state = state.UpdatePosition(RoutePosition2Point2, _segments, _route);
            state = state.TurnCommandAvailable("TurnLeft");
            state = state.TurnCommandAvailable("TurnRight");

            var result = state.UpdatePosition(RoutePosition2Point3, _segments, _route);

            result
                .Should()
                .BeOfType<UpcomingTurnState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RoutePosition2Point3);
        }

        [Fact]
        public void GivenOnRouteStateAndOnLastSegmentOfRoute_EnteringNewSegmentNotAdjacentToLastSegment_ResultingStateIsLostRouteLockState()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            _route.EnteredSegment("route-segment-3");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition3, SegmentById("route-segment-3"), _route);

            var result = state.UpdatePosition(PositionOnSegment, _segments, _route);

            result
                .Should()
                .BeOfType<LostRouteLockState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PositionOnSegment);
        }

        [Fact]
        public void GivenOnSegmentStateAndReEnteringLastSegmentOfRoute_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            _route.EnteredSegment("route-segment-3");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition3, SegmentById("route-segment-3"), _route);
            state = state.UpdatePosition(PositionOnSegment, _segments, _route); // Results in an LostRouteLockState on segment-1

            var result = state.UpdatePosition(RoutePosition3, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RoutePosition3);
        }

        [Fact]
        public void GivenOnSecondToLastSegmentOfRouteAndEnteringLastSegment_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition2, SegmentById("route-segment-2"), _route);

            var result = state.UpdatePosition(RoutePosition3, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RoutePosition3);
        }

        [Fact]
        public void GivenOnLastSegmentOfRouteAndMovingToAnotherPointOnTheSegment_ResultingStateIsOnRouteState()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            _route.EnteredSegment("route-segment-3");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition3, SegmentById("route-segment-3"), _route);

            var result = state.UpdatePosition(RoutePosition3Point2, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RoutePosition3Point2);
        }

        [Fact]
        public void GivenCompletedRouteStateAndExitingLastSegmentOfRoute_ResultingStateIsRouteCompletedState()
        {
            _route.EnteredSegment("route-segment-1");
            _route.EnteredSegment("route-segment-2");
            _route.EnteredSegment("route-segment-3");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition3, SegmentById("route-segment-3"), _route);

            var result = state.UpdatePosition(Position4, _segments, _route);

            result
                .Should()
                .BeOfType<CompletedRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(Position4);
        }

        [Fact]
        public void GivenCompletedRouteAndRouteIsLoopAndRouteHasBeenReset_ResultingStateIsOnRouteState()
        {
            var loopRoute = new PlannedRoute
            {
                RouteSegmentSequence =
                {
                    new SegmentSequence { SegmentId = "route-segment-1", NextSegmentId = "route-segment-2", TurnToNextSegment = TurnDirection.Left, Direction = SegmentDirection.AtoB},
                    new SegmentSequence { SegmentId = "route-segment-2", NextSegmentId = "route-segment-3", TurnToNextSegment = TurnDirection.Right, Direction = SegmentDirection.AtoB },
                    new SegmentSequence { SegmentId = "route-segment-3", NextSegmentId = "route-segment-1", TurnToNextSegment = TurnDirection.GoStraight, Direction = SegmentDirection.AtoB },
                }
            };
            loopRoute.EnteredSegment("route-segment-1");
            loopRoute.EnteredSegment("route-segment-2");
            loopRoute.EnteredSegment("route-segment-3");

            GameStates.GameState state = new CompletedRouteState(RiderId, ActivityId, RoutePosition3, loopRoute);
            
            // This is called by the Runner engine
            loopRoute.Reset();
            
            var result = state.UpdatePosition(RoutePosition1, _segments, loopRoute);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(RoutePosition1);
        }

        // Ignore in build
        //[Fact]
        public void OverlappingSegmentMatch()
        {
            // This test verifies that segments that cross each other
            // don't cause the state to jump back and forth between
            // those two segments.
            // This is an issue mostly in the Volcano on Watopia.
            var fileRoot = @"c:\git\RoadCaptain\src\RoadCaptain.Adapters";
            var segmentStore = new SegmentStore(fileRoot);
            var segments = segmentStore.LoadSegments(new World() { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia }, SportType.Cycling);
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk(fileRoot));
            var plannedRoute = routeStore.LoadFrom(@"C:\git\temp\zwift\RoadCaptain-troubleshoot\77-volcano-climb\DragonVsTitan.json.json");
            plannedRoute.EnteredSegment("watopia-bambino-fondo-001-after-after-after-after-after-before");
            plannedRoute.EnteredSegment("watopia-beach-island-loop-001");
            plannedRoute.EnteredSegment("watopia-bambino-fondo-004-after-before");

            var currentPosition = TrackPoint.FromGameLocation(46926.22000000, -145711.70000000, 10927.59000000, ZwiftWorldId.Watopia);
            
            var segment = segments.Single(s => s.Id == "watopia-bambino-fondo-004-after-before");
            currentPosition = segment
                .Points
                .Where(p => TrackPoint.IsCloseToQuick(p.Longitude, currentPosition))
                .Select(p => new { Point = p, Distance = p.DistanceTo(currentPosition)})                         
                .OrderBy(d => d.Distance)
                .First()
                .Point;

            var state = new OnSegmentState(1234, 5678,
                currentPosition,
                segment);

            var newPoint = TrackPoint.FromGameLocation(46673.94000000, -145711.80000000, 10926.55000000, ZwiftWorldId.Watopia);
            
            var result = state.UpdatePosition(newPoint, segments, plannedRoute);

            result
                .Should()
                .BeOfType<OnRouteState>()
                .Which
                .CurrentSegment
                .Id
                .Should()
                .Be("watopia-bambino-fondo-004-after-before");
        }

        private Segment SegmentById(string id)
        {
            return _segments.SingleOrDefault(s => s.Id == id);
        }

        private readonly List<Segment> _segments = new()
        {
            new Segment(new List<TrackPoint>
            {
                PositionOnSegment,
                OtherOnSegment
            })
            {
                Id = "segment-1"
            },
            new Segment(new List<TrackPoint>
                {
                    PositionOnAnotherSegment
                })
            {
                Id = "segment-2"
            },
            new Segment(new List<TrackPoint>()
            {
                RoutePosition1,
                RoutePosition1Point2,
                RoutePosition1Point3
            })
            {
                Id = "route-segment-1",
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.Left, "route-segment-2"),
                    new Turn(TurnDirection.Right, "route-segment-3"),
                    new Turn(TurnDirection.GoStraight, "segment-4")
                }
            },
            new Segment(new List<TrackPoint>()
            {
                RoutePosition2,
                RoutePosition2Point2,
                RoutePosition2Point3
            })
            {
                Id = "route-segment-2",
                NextSegmentsNodeA =
                {
                    new Turn(TurnDirection.GoStraight, "route-segment-1")
                },
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.Left, "segment-1"),
                    new Turn(TurnDirection.Right, "route-segment-3")
                }
            },
            new Segment(new List<TrackPoint>()
            {
                RoutePosition3,
                RoutePosition3Point2
            })
            {
                Id = "route-segment-3",
                NextSegmentsNodeA =
                {
                    new Turn(TurnDirection.GoStraight, "route-segment-2")
                },
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.GoStraight, "segment-4")
                }
            },
            new Segment(new List<TrackPoint>
            {
                Position4
            })
            {
                Id = "segment-4",
                NextSegmentsNodeA =
                {
                    new Turn(TurnDirection.GoStraight, "route-segment-3")
                },
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.GoStraight, "segment-1")
                }
            }
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
        private static readonly TrackPoint RoutePosition1Point3 = new(10, 3, 3);
        private static readonly TrackPoint RoutePosition2 = new(12, 2, 3);
        private static readonly TrackPoint RoutePosition2Point2 = new(12.5, 2.5, 3);
        private static readonly TrackPoint RoutePosition2Point3 = new(12.75, 2.75, 3);
        private static readonly TrackPoint RoutePosition3 = new(13, 3, 3);
        private static readonly TrackPoint RoutePosition3Point2 = new(13, 4, 3);
        private static readonly TrackPoint Position4 = new(14, 1, 3);

        private readonly PlannedRoute _route = new()
        {
            RouteSegmentSequence =
            {
                new SegmentSequence { SegmentId = "route-segment-1", NextSegmentId = "route-segment-2", TurnToNextSegment = TurnDirection.Left, Direction = SegmentDirection.AtoB},
                new SegmentSequence { SegmentId = "route-segment-2", NextSegmentId = "route-segment-3", TurnToNextSegment = TurnDirection.Right, Direction = SegmentDirection.AtoB },
                new SegmentSequence { SegmentId = "route-segment-3", Direction = SegmentDirection.AtoB },
            }
        };
    }
}