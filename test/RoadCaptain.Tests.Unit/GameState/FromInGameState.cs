// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromInGameState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_ResultIsPositionedState()
        {
            var result = new InGameState(1, 2)
                .UpdatePosition(PointNotOnAnySegment, Segments, Route);

            result
                .Should()
                .BeOfType<PositionedState>()
                .Which
                .CurrentPosition
                .Should()
                .Be(PointNotOnAnySegment);
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentState()
        {
            var result = new InGameState(1, 2)
                .UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .BeOfType<OnSegmentState>();
        }

        [Fact]
        public void GivenPositionOnSegment_ResultIsOnSegmentStateForSegmentOne()
        {
            var result = new InGameState(1, 2)
                .UpdatePosition(Segment1Point1, Segments, Route);

            result
                .As<OnSegmentState>()
                .CurrentSegment
                .Id
                .Should()
                .Be(Segment1.Id);
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
        public void TurnCommandAvailable_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => new InGameState(1, 2).TurnCommandAvailable("left");

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }
    }
}

