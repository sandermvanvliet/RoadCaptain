using System;
using FluentAssertions;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenInvalidCredentialsStateIsReceived : EngineTest
    {
        [StaFact]
        public void NavigationTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_navigationTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_navigationTask")
                .Should()
                .BeNull();
        }

        [StaFact]
        public void ListenerTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_listenerTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_listenerTask")
                .Should()
                .BeNull();
        }

        [StaFact]
        public void InitiatorTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_initiatorTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_initiatorTask")
                .Should()
                .BeNull();
        }

        [StaFact]
        public void MessageHandlingTaskIsCleanedUp()
        {
            GivenTaskIsRunning("_messageHandlingTask");

            GivenInvalidCredentialsStateIsReceived();

            TheTaskWithName("_messageHandlingTask")
                .Should()
                .BeNull();
        }

        [StaFact]
        public void ErrorDialogIsShown()
        {
            GivenInvalidCredentialsStateIsReceived();

            WindowService
                .ErrorDialogInvocations
                .Should()
                .Be(1);
        }

        [StaFact]
        public void MainWindowIsShown()
        {
            GivenInvalidCredentialsStateIsReceived();

            WindowService
                .ShownWindows
                .Should()
                .Contain(typeof(MainWindow))
                .And
                .HaveCount(1);
        }

        private void GivenInvalidCredentialsStateIsReceived()
        {
            ReceiveGameState(new InvalidCredentialsState(new Exception("BANG!")));
        }
    }
}
