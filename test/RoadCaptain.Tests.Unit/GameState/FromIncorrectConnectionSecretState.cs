// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromIncorrectConnectionSecretState : StateTransitionTestBase
    {
        [Fact]
        public void GivenPosition_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            startingState
                .UpdatePosition(TrackPoint.Unknown, Segments, Route)
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenLeftTurnAvailable_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            startingState
                .TurnCommandAvailable("turnleft")
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenEnteringGame_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            startingState
                .EnterGame(1, 2)
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenLeavingGame_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            startingState
                .LeaveGame()
                .Should()
                .Be(startingState);
        }

        private IncorrectConnectionSecretState GivenStartingState()
        {
            return new IncorrectConnectionSecretState();
        }
    }
}

