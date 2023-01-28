// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.GameStates;
using Serilog.Events;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenWaitingForConnectionStateIsReceived : EngineTest
    {
        [Fact]
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

        // TODO: Re-enable this test
        //[Fact]
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

        [Fact]
        public void GivenNoWindowIsActive_CloseIsNotCalled()
        {
            GivenConnectedToZwiftStateReceived();

            WindowService
                .ClosedWindows
                .Should()
                .BeEmpty();
        }

        [Fact]
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

        [Fact]
        public void GivenNoRouteIsLoaded_InGameWindowIsNotShown()
        {
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
                Name = "test",
                World = new World { Id = "watopia" }
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { Direction = SegmentDirection.AtoB, SegmentId = "watopia-beach-island-loop-001" });
            SetFieldValueByName("_loadedRoute", plannedRoute);
        }
    }
}

