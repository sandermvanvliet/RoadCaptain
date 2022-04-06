using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Engine
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
            var loggedInState = new LoggedInState("token");

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