// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromReadyToGoState : StateTransitionTestBase
    {
        [Fact]
        public void GivenEnterGame_InGameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.EnterGame(1, 2);

            result
                .Should()
                .BeOfType<InGameState>()
                .Which
                .RiderId
                .Should()
                .Be(1);
        }

        [Fact]
        public void GivenLeaveGame_ConnectedToZwiftStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.LeaveGame();

            result
                .Should()
                .BeOfType<ConnectedToZwiftState>();
        }

        [Fact]
        public void GivenPosition_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.UpdatePosition(TrackPoint.Unknown, Segments, Route);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void GivenLeftTurnAvailable_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.TurnCommandAvailable("turnleft");

            result
                .Should()
                .Be(startingState);
        }

        private ReadyToGoState GivenStartingState()
        {
            return new ReadyToGoState();
        }
    }
}
