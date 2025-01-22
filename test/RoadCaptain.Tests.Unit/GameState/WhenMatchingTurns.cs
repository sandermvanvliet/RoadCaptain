// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenMatchingTurns
    {
        [Theory]
        [InlineData(TurnDirection.Left, TurnDirection.Right, TurnDirection.GoStraight, TurnDirection.Right)]
        [InlineData(TurnDirection.Left, TurnDirection.GoStraight, TurnDirection.GoStraight, TurnDirection.GoStraight)]
        [InlineData(TurnDirection.GoStraight, TurnDirection.Right, TurnDirection.GoStraight, TurnDirection.GoStraight)]

        [InlineData(TurnDirection.Left, TurnDirection.Right, TurnDirection.Left, TurnDirection.Left)]
        [InlineData(TurnDirection.Left, TurnDirection.GoStraight, TurnDirection.Left, TurnDirection.Left)]
        [InlineData(TurnDirection.GoStraight, TurnDirection.Right, TurnDirection.Left, TurnDirection.GoStraight)]

        [InlineData(TurnDirection.Left, TurnDirection.Right, TurnDirection.Right, TurnDirection.Right)]
        [InlineData(TurnDirection.Left, TurnDirection.GoStraight, TurnDirection.Right, TurnDirection.GoStraight)]
        [InlineData(TurnDirection.GoStraight, TurnDirection.Right, TurnDirection.Right, TurnDirection.Right)]
        public void GivenCommandsAndNextTurn_TurnCommandIsExpectedOne(
            TurnDirection commandOne,
            TurnDirection commandTwo,
            TurnDirection nextTurn,
            TurnDirection expectedCommand)
        {
            var commands = new List<TurnDirection> { commandOne, commandTwo };

            var turnCommand = NavigationUseCase.TurnCommandFor(commands, nextTurn);

            turnCommand
                .Should()
                .Be(expectedCommand);
        }
    }
}
