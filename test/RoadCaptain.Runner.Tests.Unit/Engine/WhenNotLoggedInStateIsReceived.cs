using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class WhenNotLoggedInStateIsReceived : EngineTest
    {
        public WhenNotLoggedInStateIsReceived()
        {
            SetFieldValueByName("_initiatorTask", TaskWithCancellation.Start(token => { }));
            SetFieldValueByName("_listenerTask", TaskWithCancellation.Start(token => { }));
        }

        [Fact]
        public void ZwiftConnectionListenerIsCleanedUp()
        {
            GivenNotLoggedInStateIsReceived();

            GetTaskByFieldName("_listenerTask")
                .Should()
                .BeNull();
        }

        [Fact]
        public void ZwiftConnectionInitiatorIsCleanedUp()
        {
            GivenNotLoggedInStateIsReceived();

            GetTaskByFieldName("_initiatorTask")
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

        private void GivenNotLoggedInStateIsReceived()
        {
            ReceiveGameState(new NotLoggedInState());
        }
    }
}