// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class FromErrorState : StateTransitionTestBase
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
        public void GivenPositionOnSegment_SameStateIsReturned()
        {
            var startingState = GivenStartingState();
            
            var result = startingState.UpdatePosition(Segment1Point1, Segments, Route);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void EnteringGameWithRiderAndActivityId_SameStateIsReturned()
        {
            var startingState = GivenStartingState();

            var result = startingState.EnterGame(1, 2);

            result
                .Should()
                .Be(startingState);
        }

        [Fact]
        public void LeavingGame_SameStateIsReturned()
        {
            var startingState = GivenStartingState();
            
            var result = startingState.LeaveGame();

            result
                .Should()
                .Be(startingState);
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

        private ErrorState GivenStartingState()
        {
            return new ErrorState(new Exception("BANG!"));
        }
    }
}

