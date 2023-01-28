// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenHandlingWatopiaThreeWayJunctions
    {
        [Fact]
        public void GivenCurrentSegmentIsMarinaToEpicAndLeftRightTurnsAreAvailable_ResultingStateIsUpcomingTurnState()
        {
            _route.EnteredSegment("watopia-bambino-fondo-003-before-before");
            GameStates.GameState state = new OnRouteState(RiderId, ActivityId, RoutePosition1, SegmentById("watopia-bambino-fondo-003-before-before"), _route, SegmentDirection.AtoB, 0, 0, 0);
            state = state.UpdatePosition(RoutePosition1Point2, _segments, _route);
            state = state.TurnCommandAvailable("TurnLeft");
            state = state.TurnCommandAvailable("TurnRight");

            state
                .Should()
                .BeOfType<UpcomingTurnState>();

        }

        private const uint RiderId = 56789;
        private const int ActivityId = 1234;

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
                Id = "watopia-bambino-fondo-003-before-before",
                NextSegmentsNodeB =
                {
                    new Turn(TurnDirection.Left, "watopia-bambino-fondo-003-before-after"),
                    new Turn(TurnDirection.Right, "watopia-ocean-lava-cliffside-loop-001"),
                    new Turn(TurnDirection.GoStraight, "watopia-big-loop-rev-001-before-after")
                }
            },
            new Segment(new List<TrackPoint>()
            {
                RoutePosition2,
                RoutePosition2Point2,
                RoutePosition2Point3
            })
            {
                Id = "watopia-bambino-fondo-003-before-after",
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
                new SegmentSequence { SegmentId = "watopia-bambino-fondo-003-before-before", NextSegmentId = "watopia-bambino-fondo-003-before-after", TurnToNextSegment = TurnDirection.Left, Direction = SegmentDirection.AtoB},
                new SegmentSequence { SegmentId = "watopia-bambino-fondo-003-before-after", Direction = SegmentDirection.AtoB },
            }
        };

        public WhenHandlingWatopiaThreeWayJunctions()
        {
            // We need valid indexes on all points
            foreach (var segment in _segments)
            {
                for (var index = 0; index < segment.Points.Count; index++)
                {
                    var point = segment.Points[index];
                    point.Index = index;
                    point.Segment = segment;
                }
            }
        }
    }
}
