using FluentAssertions;
using RoadCaptain.GameStates;
using Serilog.Events;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class WhenWaitingForConnectionStateIsReceived : EngineTest
    {
        [StaFact]
        public void MessageIsLogged()
        {
            GivenConnectedToZwiftStateReceived();

            InMemorySink
                .Should()
                .HaveMessage("Waiting for connection from Zwift")
                .Appearing()
                .Once()
                .WithLevel(LogEventLevel.Information);
        }

        [StaFact]
        public void GivenRouteIsLoadedAndMainWindowIsActive_MainWindowIsClosed()
        {
            GivenLoadedRoute();
            WindowService.ShowMainWindow();

            GivenConnectedToZwiftStateReceived();

            WindowService
                .ClosedWindows
                .Should()
                .Contain(typeof(MainWindow))
                .And
                .HaveCount(1);
        }

        [StaFact]
        public void GivenNoWindowIsActive_CloseIsNotCalled()
        {
            GivenConnectedToZwiftStateReceived();

            WindowService
                .ClosedWindows
                .Should()
                .BeEmpty();
        }

        [StaFact]
        public void GivenRouteIsLoaded_InGameWindowIsShown()
        {
            GivenLoadedRoute();
            WindowService.ShowMainWindow();

            GivenConnectedToZwiftStateReceived();

            WindowService
                .ShownWindows
                .Should()
                .Contain(typeof(InGameNavigationWindow));
        }

        [StaFact]
        public void GivenNoRouteIsLoaded_InGameWindowIsNotShown()
        {
            //WindowService.ShowMainWindow();

            GivenConnectedToZwiftStateReceived();

            WindowService
                .ShownWindows
                .Should()
                .BeEmpty();
        }

        private void GivenConnectedToZwiftStateReceived()
        {
            ReceiveGameState(new WaitingForConnectionState());
        }

        private void GivenLoadedRoute()
        {
            var plannedRoute = new PlannedRoute
            {
                ZwiftRouteName = "test"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { Direction = SegmentDirection.AtoB, SegmentId = "watopia-beach-island-loop-001"});
            SetFieldValueByName("_loadedRoute", plannedRoute);
        }
    }
}
