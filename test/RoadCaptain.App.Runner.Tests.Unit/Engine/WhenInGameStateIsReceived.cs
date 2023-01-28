// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Serilog.Events;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenInGameStateIsReceived : EngineTest
    {
        [Fact]
        public void NavigationTaskIsStarted()
        {
            GivenInGameStateIsReceived();

            TheTaskWithName("_navigationTask")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenNavigationTaskAlreadyStarted_ItIsNotStartedAgain()
        {
            var navigationTask = GivenTaskIsRunning("_navigationTask");

            GivenInGameStateIsReceived();

            TheTaskWithName("_navigationTask")
                .Should()
                .Be(navigationTask);
        }

        [Fact]
        public void MessageIsLogged()
        {
            GivenInGameStateIsReceived();

            InMemorySink
                .Should()
                .HaveMessage("User entered the game")
                .Appearing()
                .Once()
                .WithLevel(LogEventLevel.Information);
        }

        private void GivenInGameStateIsReceived()
        {
            ReceiveGameState(new InGameState(1, 2));
        }
    }
}

