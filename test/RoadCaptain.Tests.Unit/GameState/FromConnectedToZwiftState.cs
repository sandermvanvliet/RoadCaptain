// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromConnectedToZwiftState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPositionNotOnSegment_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.UpdatePosition(PointNotOnAnySegment, Segments, Route);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenPositionOnSegment_InvalidStateTransitionExceptionIsThrown()
        {
            var startingState = GivenStartingState();

            var result = startingState.UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void EnteringGameWithRiderAndActivityId_ResultIsInGameState()
        {
            var result = GivenStartingState().EnterGame(1, 2);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .RiderId
                .Should()
                .Be(1);
        }

        [Fact]
        public void LeavingGame_InvalidStateTransitionExceptionIsThrown()
        {
            var action = () => GivenStartingState().LeaveGame();

            action
                .Should()
                .Throw<InvalidStateTransitionException>();
        }

        [Fact]
        public void TurnCommandAvailable_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.TurnCommandAvailable("turnleft");

            result
                .Should()
                .Be(startingState);
        }

        private ConnectedToZwiftState GivenStartingState()
        {
            return new ConnectedToZwiftState();
        }
    }
}

