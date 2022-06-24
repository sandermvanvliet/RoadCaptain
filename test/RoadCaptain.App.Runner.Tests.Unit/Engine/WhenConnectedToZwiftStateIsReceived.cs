using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.GameStates;
using Serilog.Events;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenConnectedToZwiftStateIsReceived : EngineTest
    {
        [Fact]
        public void RouteIsLoaded()
        {
            GivenConnectedToZwiftStateReceived();

            LoadedRoute
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void MessageHandlerIsStarted()
        {
            GivenConnectedToZwiftStateReceived();

            TheTaskWithName("_messageHandlingTask")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenMessageHandlerAlreadyStarted_ItIsNotStartedAgain()
        {
            var task = GivenTaskIsRunning("_messageHandlingTask");

            GivenConnectedToZwiftStateReceived();

            TheTaskWithName("_messageHandlingTask")
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

        [Fact]
        public void GivenPreviousStateWasOnSegmentState_MainWindowIsShown()
        {
            ReceiveGameState(new OnSegmentState(1234, 12345, new TrackPoint(1,2,3), new Segment(new List<TrackPoint>())));

            GivenConnectedToZwiftStateReceived();

            WindowService.ShownWindows.Should().Contain(typeof(MainWindow));
        }

        [Fact]
        public void GivenPreviousStateWasOnSegmentState_InGameWindowIsClosed()
        {
            WindowService.ShowInGameWindow(new InGameNavigationWindowViewModel(new InGameWindowModel(new List<Segment>()), new List<Segment>(), null));

            ReceiveGameState(new OnSegmentState(1234, 12345, new TrackPoint(1,2,3), new Segment(new List<TrackPoint>())));

            GivenConnectedToZwiftStateReceived();

            WindowService.ClosedWindows.Should().Contain(typeof(InGameNavigationWindow));
        }

        private void GivenConnectedToZwiftStateReceived()
        {
            ReceiveGameState(new ConnectedToZwiftState());
        }
    }
}
