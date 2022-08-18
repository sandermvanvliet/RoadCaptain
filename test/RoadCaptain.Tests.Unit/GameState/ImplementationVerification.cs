﻿using System.Linq;
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
    }
}