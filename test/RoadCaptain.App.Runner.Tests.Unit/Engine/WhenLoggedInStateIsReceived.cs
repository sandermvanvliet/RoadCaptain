using FluentAssertions;
using RoadCaptain.GameStates;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenLoggedInStateIsReceived : EngineTest
    {
        [Fact]
        public void MessageIsLogged()
        {
            GivenLoggedInStateIsReceived();

            InMemorySink
                .Should()
                .HaveMessage("User logged in")
                .Appearing()
                .Once();
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

        private void GivenLoggedInStateIsReceived()
        {
            ReceiveGameState(new LoggedInState());
        }
    }
}