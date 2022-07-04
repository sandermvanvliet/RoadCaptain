using System.Linq;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class WhenMatchingTurns
    {
        [Theory]
        [InlineData(TurnDirection.Left, TurnDirection.Right, TurnDirection.GoStraight, TurnDirection.Right)]
        [InlineData(TurnDirection.Left, TurnDirection.GoStraight, TurnDirection.GoStraight, TurnDirection.GoStraight)]
        [InlineData(TurnDirection.GoStraight, TurnDirection.Right, TurnDirection.GoStraight, TurnDirection.Right)]

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
            var commands = new[] { commandOne, commandTwo };

            var turnCommand = TurnCommandFor(commands, nextTurn);

            turnCommand
                .Should()
                .Be(expectedCommand);
        }

        private TurnDirection TurnCommandFor(TurnDirection[] commands, TurnDirection nextTurn)
        {
            if (nextTurn == TurnDirection.Left)
            {
                if (commands.Contains(TurnDirection.Left))
                {
                    if (commands.Contains(TurnDirection.GoStraight) ||
                        commands.Contains(TurnDirection.Right))
                    {
                        return TurnDirection.Left;
                    }
                }

                return TurnDirection.GoStraight;
            }

            if (nextTurn == TurnDirection.GoStraight ||
                nextTurn == TurnDirection.Right)
            {
                if (commands.Contains(TurnDirection.Right))
                {
                    if (commands.Contains(TurnDirection.GoStraight) ||
                        commands.Contains(TurnDirection.Left))
                    {
                        return TurnDirection.Right;
                    }
                }
                
                return TurnDirection.GoStraight;
            }

            return TurnDirection.None;
        }
    }
}