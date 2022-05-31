using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenCompletedRouteStateIsReceived : EngineTest
    {
        [Fact]
        public void GivenRouteCompletes_EndActivityCommandIsSent()
        {
            GivenCompletedRouteStateReceived();

            SentCommands
                .Should()
                .Contain($"ENDACTIVITY;RoadCaptain: {_route.Name}");
        }

        private readonly PlannedRoute _route = new PlannedRoute { Name = "Test Route" };

        private void GivenCompletedRouteStateReceived()
        {
            ReceiveGameState(new CompletedRouteState(1, 2, new TrackPoint(1,2,3), _route));
        }
    }
}