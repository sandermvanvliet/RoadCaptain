// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenNotLoggedInStateIsReceived : EngineTest
    {
        public WhenNotLoggedInStateIsReceived()
        {
            GivenTaskIsRunning("_initiatorTask");
            GivenTaskIsRunning("_listenerTask");
            GivenTaskIsRunning("_messageHandlingTask");
        }

        [Fact]
        public void ZwiftConnectionListenerIsCleanedUp()
        {
            GivenNotLoggedInStateIsReceived();

            TheTaskWithName("_listenerTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void ZwiftConnectionInitiatorIsCleanedUp()
        {
            GivenNotLoggedInStateIsReceived();

            TheTaskWithName("_initiatorTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void PreviousGameStateIsSetToLoggedInState()
        {
            var loggedInState = new LoggedInState();

            ReceiveGameState(loggedInState);

            GetFieldValueByName("_previousGameState")
                .Should()
                .Be(loggedInState);
        }

        [Fact]
        public void MessageHandlingIsCleanedUp()
        {
            GivenNotLoggedInStateIsReceived();

            TheTaskWithName("_messageHandlingTask")
                .Should()
                .BeNull();
        }

        private void GivenNotLoggedInStateIsReceived()
        {
            ReceiveGameState(new NotLoggedInState());
        }
    }
}
