// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class ImplementationVerification
    {
        [Fact]
        public void NoGameStateDependsOnAnother()
        {
            var gameStateType = typeof(GameStates.GameState);

            var gameStateTypes = gameStateType
                .Assembly
                .GetTypes()
                .Where(type => gameStateType.IsAssignableFrom(type) && 
                               type != gameStateType)
                .ToList();

            var typesNotInheritingFromGameState = gameStateTypes
                .Where(type => type.BaseType != gameStateType)
                .Select(type => type.Name.Split(".").Last())
                .ToList();

            typesNotInheritingFromGameState
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void AllGameStatesAreSealed()
        {
            var gameStateType = typeof(GameStates.GameState);

            var gameStateTypes = gameStateType
                .Assembly
                .GetTypes()
                .Where(type => gameStateType.IsAssignableFrom(type) && 
                               type != gameStateType)
                .ToList();

            var notSealedStates = gameStateTypes
                .Where(type => !type.IsSealed)
                .Select(type => type.Name.Split(".").Last())
                .ToList();

            notSealedStates
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void AllGameStatesHaveATestClass()
        {
            var gameStateType = typeof(GameStates.GameState);

            var gameStateTypes = gameStateType
                .Assembly
                .GetTypes()
                .Where(type => gameStateType.IsAssignableFrom(type) && 
                               type != gameStateType)
                .ToList();

            var stateTransitionTestType = typeof(StateTransitionTestBase);

            var testTypes = stateTransitionTestType
                .Assembly
                .GetTypes()
                .Where(type => stateTransitionTestType.IsAssignableFrom(type) && 
                               type != stateTransitionTestType)
                .Select(type => type.Name.Split(".").Last())
                .ToList();

            var missingTestStates = new List<string>();

            foreach (var gameState in gameStateTypes)
            {
                var gameStateName = gameState.Name.Split(".").Last();
                var expectedTestName = $"From{gameStateName}";

                if (!testTypes.Contains(expectedTestName))
                {
                    missingTestStates.Add(gameStateName);
                }
            }

            missingTestStates
                .Should()
                .BeEmpty();
        }
    }
}

