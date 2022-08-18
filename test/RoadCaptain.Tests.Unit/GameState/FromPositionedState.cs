using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromPositionedState
    {
        private readonly PlannedRoute _route;
        private readonly List<Segment> _segments;
        private readonly TrackPoint _startingPosition;

        public FromPositionedState()
        {
            _segments = new List<Segment>();
            _route = SegmentSequenceBuilder.TestLoopOne();
            _startingPosition = new TrackPoint(1, 2, 3, ZwiftWorldId.Watopia);
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
            var position = new TrackPoint(2, 3, 4, ZwiftWorldId.Watopia);
            _segments.Add(new Segment(new List<TrackPoint> { position })
            {
                Id = "segment-1"
            });

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentStateForSegmentOne()
        {
            var position = new TrackPoint(2, 3, 4, ZwiftWorldId.Watopia);
            _segments.Add(new Segment(new List<TrackPoint> { position })
            {
                Id = "segment-1"
            });

            var result = GivenStartingState()
                .UpdatePosition(position, _segments, _route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
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

        private PositionedState GivenStartingState()
        {
            return new PositionedState(1, 2, _startingPosition);
        }
    }
}