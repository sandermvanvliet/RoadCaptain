using FluentAssertions;
using RoadCaptain.GameStates;
using Serilog.Events;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class WhenConnectedToZwiftStateIsReceived : EngineTest
    {
        [Fact]
        public void RouteIsLoaded()
        {
            GivenConnectedToZwiftStateReceived();

            
        }

        [Fact]
        public void MessageHandlerIsStarted()
        {
            GivenConnectedToZwiftStateReceived();

            GetTaskByFieldName("_messageHandlingTask")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenMessageHandlerAlreadyStarted_ItIsNotStartedAgain()
        {
            var task = GivenTaskIsRunning("_messageHandlingTask");

            GivenConnectedToZwiftStateReceived();

            GetTaskByFieldName("_messageHandlingTask")
                .Should()
                .Be(task);
        }

        [Fact]
        public void MessageIsLogged()
        {
            GivenConnectedToZwiftStateReceived();

            InMemorySink
                .Should()
                .HaveMessage("Connected to Zwift")
                .Appearing()
                .Once()
                .WithLevel(LogEventLevel.Information);
        }

        private void GivenConnectedToZwiftStateReceived()
        {
            ReceiveGameState(new ConnectedToZwiftState());
        }
    }
}
