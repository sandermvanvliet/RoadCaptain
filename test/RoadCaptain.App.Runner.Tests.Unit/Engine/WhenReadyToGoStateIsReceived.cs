using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenReadyToGoStateIsReceived : EngineTest
    {

        [Fact]
        public void ZwiftConnectionListenerIsStarted()
        {
            GivenReadyToGoStateIsReceived();

            TheTaskWithName("_listenerTask")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void ZwiftConnectionInitiatorIsStarted()
        {
            GivenReadyToGoStateIsReceived();

            TheTaskWithName("_initiatorTask")
                .Should()
                .NotBeNull();
        }

        private void GivenReadyToGoStateIsReceived()
        {
            ReceiveGameState(new ReadyToGoState());
        }
    }
}