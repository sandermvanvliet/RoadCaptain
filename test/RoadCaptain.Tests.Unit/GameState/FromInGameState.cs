using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromInGameState
    {
        private readonly List<Segment> _segments;
        private readonly PlannedRoute _route;

        public FromInGameState()
        {
            _segments = new List<Segment>();
            _route = SegmentSequenceBuilder.TestLoopOne();
        }

        [Fact]
        public void GivenPositionNotOnSegment_ResultIsPositionedState()
        {
            var position = new TrackPoint(1, 2, 3, ZwiftWorldId.Watopia);

            var result = new InGameState(1, 2)
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
            var position = new TrackPoint(1, 2, 3, ZwiftWorldId.Watopia);
            _segments.Add(new Segment(new List<TrackPoint>{ position})
            {
                Id = "segment-1"
            });

            var result = new InGameState(1, 2)
                .UpdatePosition(position, _segments, _route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentStateForSegmentOne()
        {
            var position = new TrackPoint(1, 2, 3, ZwiftWorldId.Watopia);
            _segments.Add(new Segment(new List<TrackPoint>{ position})
            {
                Id = "segment-1"
            });

            var result = new InGameState(1, 2)
                .UpdatePosition(position, _segments, _route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be("segment-1");
        }

        [Fact]
        public void EnteringGameWithSameRiderAndActivityId_OriginalStateIsReturned()
        {
            var startingState = new InGameState(1, 2);

            var result = startingState
                .EnterGame(1, 2);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void EnteringGameWithSameRiderAndDifferentActivityId_NewInGameStateIsReturned()
        {
            var startingState = new InGameState(1, 2);

            var result = startingState
                .EnterGame(1, 3);

            result
                .Should()
                .NotBe(startingState)
                .And
                .BeOfType<InGameState>()
                .Which
                .ActivityId
                .Should()
                .Be(3);
        }

        [Fact]
        public void EnteringGameWithDifferentRiderAndSameActivityId_NewInGameStateIsReturned()
        {
            var startingState = new InGameState(1, 2);

            var result = startingState
                .EnterGame(2, 2);

            result
                .Should()
                .NotBe(startingState)
                .And
                .BeOfType<InGameState>()
                .Which
                .ActivityId
                .Should()
                .Be(2);
        }

        [Fact]
        public void LeavingGame_ConnectedToZwiftStateIsReturned()
        {
            var result = new InGameState(1, 2).LeaveGame();

            result
                .Should()
                .BeOfType<ConnectedToZwiftState>();
        }

        [Fact]
        public void TurnCommandAvaialble_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => new InGameState(1, 2).TurnCommandAvailable("left");

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }
    }
}
