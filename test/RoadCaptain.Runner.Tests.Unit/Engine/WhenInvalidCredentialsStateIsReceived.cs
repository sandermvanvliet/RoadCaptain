using System;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class WhenInvalidCredentialsStateIsReceived : EngineTest
    {
        [Fact]
        public void NavigationTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_navigationTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_navigationTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void ListenerTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_listenerTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_listenerTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void InitiatorTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_initiatorTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_initiatorTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void MessageHandlingTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_messageHandlingTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_messageHandlingTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void ErrorDialogIsShown()
        {
            GivenInvalidCredentialsStateIsReceived();

            WindowService
                .ErrorDialogInvocations
                .Should()
                .Be(1);
        }

        [Fact]
        public void MainWindowIsShown()
        {
            GivenInvalidCredentialsStateIsReceived();

            WindowService
                .MainWindowInvocations
                .Should()
                .Be(1);
        }

        private void GivenInvalidCredentialsStateIsReceived()
        {
            ReceiveGameState(new InvalidCredentialsState(new Exception("BANG!")));
        }
    }
}
