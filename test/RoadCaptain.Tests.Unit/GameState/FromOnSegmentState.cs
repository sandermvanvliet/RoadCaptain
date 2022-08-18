using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromOnSegmentState
    {
        private readonly PlannedRoute _route;
        private readonly List<Segment> _segments;
        private readonly TrackPoint _startingPosition;

        public FromOnSegmentState()
        {
            _startingPosition = new TrackPoint(1, 2, 3, ZwiftWorldId.Watopia);
            _segments = new List<Segment>
            {
                new(new List<TrackPoint> { _startingPosition })
                {
                    Id = "segment-1"
                },
                new(new List<TrackPoint>
                {
                    new(2, 1, 0, ZwiftWorldId.Watopia),
                    new(2, 2, 0, ZwiftWorldId.Watopia)
                })
                {
                    Id = "segment-2"
                },
                new(new List<TrackPoint>
                {
                    new(3, 1, 0, ZwiftWorldId.Watopia),
                    new(3, 2, 0, ZwiftWorldId.Watopia),
                    new(3, 3, 0, ZwiftWorldId.Watopia)
                })
                {
                    Id = "route-segment-1"
                }
            };

            _route = new PlannedRoute
            {
                RouteSegmentSequence =
                {
                    new SegmentSequence
                    {
                        SegmentId = "route-segment-1", 
                        Direction = SegmentDirection.AtoB,
                        Type = SegmentSequenceType.Regular
                    }
                }
            };
        }

        [Fact]
        public void GivenPositionNotOnSegment_ResultIsPositionedState()
        {
            var position = new TrackPoint(2, 3, 4, ZwiftWorldId.Watopia);

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(position);
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentState()
        {
            var position = new TrackPoint(2, 1, 0, ZwiftWorldId.Watopia);

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenNextPositionOnSegment_ResultIsOnSegmentStateWithDirectionAtoB()
        {
            var position = new TrackPoint(2, 1, 0, ZwiftWorldId.Watopia);

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route)
                .UpdatePosition(new TrackPoint(2,2, 0, ZwiftWorldId.Watopia), _segments, _route);

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
            var position = new TrackPoint(2, 1, 0, ZwiftWorldId.Watopia);

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-2");
        }

        [Fact]
        public void GivenPositionOnSegmentAndFirstSegmentOfRoute_ResultIsOnRouteState()
        {
            var position = new TrackPoint(3, 1, 0, ZwiftWorldId.Watopia);
            
            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .Should()
                .BeOfType<OnRouteState>();
        }

        [Fact]
        public void GivenPositionOnSegmentAndFirstSegmentOfRoute_RouteIsStarted()
        {
            var position = new TrackPoint(3, 1, 0, ZwiftWorldId.Watopia);
            
            GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            _route
                .HasStarted
                .Should()
                .BeTrue();
            
            _route
                .CurrentSegmentId
                .Should()
                .Be("route-segment-1");
        }

        [Fact]
        public void GivenRouteHasStartedAndPositionOnSegmentAndFirstSegmentOfRoute_InvalidStateTransitionExceptionIsThrown()
        {
            _route.EnteredSegment("route-segment-1");

            var position = new TrackPoint(3, 2, 0, ZwiftWorldId.Watopia);
            
            var action = () => GivenStartingState().UpdatePosition(position, _segments, _route);

            action
                .Should()
                .Throw<InvalidStateTransitionException>("from a started route you can only go to OnRouteState from LostRouteLockState or UpcomingTurnState");
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

        private OnSegmentState GivenStartingState()
        {
            return new OnSegmentState(1, 2, _startingPosition, _segments.First(), SegmentDirection.AtoB, 0, 0, 0);
        }
    }
}