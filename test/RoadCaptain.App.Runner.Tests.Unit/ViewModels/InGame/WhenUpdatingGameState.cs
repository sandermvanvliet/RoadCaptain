// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.InGame
{
    public class WhenUpdatingGameState
    {
        private readonly World _world = new() { Id = "testworld", Name = "TestWorld" };

        [Fact]
        public void GivenWaitingForConnectionState_WaitReasonIsWaitingForConnection()
        {
            WhenUpdating(new WaitingForConnectionState());

            TheCallToAction
                .WaitingReason
                .Should()
                .Be("Waiting for Zwift...");
        }

        [Fact]
        public void GivenWaitingForConnectionState_InstructionIsStartZwift()
        {
            WhenUpdating(new WaitingForConnectionState());

            TheCallToAction
                .InstructionText
                .Should()
                .Be($"Start Zwift and start cycling in {_world.Name} on route: ");
        }

        [Fact]
        public void GivenWaitingForConnectionAndInGameStateIsReceived_WaitReasonIsEnteredGame()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new InGameState(1, 2));

            TheCallToAction
                .WaitingReason
                .Should()
                .Be("Entered the game");
        }

        [Fact]
        public void GivenWaitingForConnectionAndInGameStateIsReceived_InstructionIsStartPedaling()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new InGameState(1, 2));
            
            TheCallToAction.InstructionText.Should().Be("Start pedaling!");
        }

        [Fact]
        public void GivenInGameAndWaitingForConnectionStateIsReceived_WaitReasonIsConnectionLost()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new WaitingForConnectionState());

            TheCallToAction
                .WaitingReason
                .Should()
                .Be("Connection with Zwift was lost, waiting for reconnect...");
        }

        [Fact]
        public void GivenInGameAndWaitingForConnectionStateIsReceived_InstructionIsEmpty()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new WaitingForConnectionState());

            TheCallToAction
                .InstructionText
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenInGameAndConnectedStateIsReceived_WaitReasonIsConnectedWithZwift()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new ConnectedToZwiftState());

            TheCallToAction
                .WaitingReason
                .Should()
                .Be("Connected with Zwift");
        }

        [Fact]
        public void GivenInGameAndConnectedStateIsReceived_InstructionIsStartRiding()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new ConnectedToZwiftState());

            TheCallToAction
                .InstructionText
                .Should()
                .Be($"Start cycling in {_world.Name} on route: ");
        }

        [Fact]
        public void GivenWaitingForConnectionAndConnectedStateIsReceived_WaitReasonIsConnectedWithZwift()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new ConnectedToZwiftState());

            TheCallToAction
                .WaitingReason
                .Should()
                .Be("Connected with Zwift");
        }

        [Fact]
        public void GivenWaitingForConnectionAndConnectedStateIsReceived_InstructionIsStartRiding()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new ConnectedToZwiftState());
            
            TheCallToAction
                .InstructionText
                .Should()
                .Be($"Start cycling in {_world.Name} on route: ");
        }

        [Fact]
        public void GivenWaitingForConnectionAndOnRouteStateIsReceived_WaitReasonIsEmpty()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()), new PlannedRoute(), SegmentDirection.AtoB, 0, 0, 0));

            _viewModel.CallToAction?.WaitingReason.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GivenErrorStateIsReceived_WaitReasonIsOopsSomethingWentWrong()
        {
            WhenUpdating(new ErrorState(new Exception("BANG")));

            TheCallToAction.WaitingReason.Should().Be("Oops! Something went wrong...");
        }

        [Fact]
        public void GivenErrorStateIsReceived_InstructionIsPleaseReportBug()
        {
            WhenUpdating(new ErrorState(new Exception("BANG")));

            TheCallToAction.InstructionText.Should().Be("BANG.\nPlease report a bug on Github");
        }

        [Fact]
        public void GivenIncorrectConnectionSecretStateIsReceived_WaitReasonIsOopsSomethingWentWrong()
        {
            WhenUpdating(new IncorrectConnectionSecretState());

            TheCallToAction.WaitingReason.Should().Be("Zwift connection failed");
        }

        [Fact]
        public void GivenIncorrectConnectionSecretStateIsReceived_InstructionIsPleaseReportBug()
        {
            WhenUpdating(new IncorrectConnectionSecretState());

            TheCallToAction.InstructionText.Should().Be("Retrying connection...");
        }

        [Fact]
        public void GivenPositionedStateIsReceived_WaitReasonIsRidingToStartOfRoute()
        {
            WhenUpdating(new PositionedState(1,2, TrackPoint.Unknown));

            TheCallToAction.WaitingReason.Should().Be("Riding to start of route");
        }

        [Fact]
        public void GivenPositionedStateIsReceived_InstructionIsKeepPedaling()
        {
            WhenUpdating(new PositionedState(1,2, TrackPoint.Unknown));

            TheCallToAction.InstructionText.Should().Be("Keep pedaling!");
        }

        [Fact]
        public void GivenOnSegmentStateIsReceived_WaitReasonIsRidingToStartOfRoute()
        {
            WhenUpdating(new OnSegmentState(1, 2, TrackPoint.Unknown, new Segment(new List<TrackPoint>()), SegmentDirection.AtoB, 0, 0, 0));

            TheCallToAction.WaitingReason.Should().Be("Riding to start of route");
        }

        [Fact]
        public void GivenOnSegmentStateIsReceived_InstructionIsKeepPedaling()
        {
            WhenUpdating(new OnSegmentState(1, 2, TrackPoint.Unknown, new Segment(new List<TrackPoint>()), SegmentDirection.AtoB, 0, 0, 0));

            TheCallToAction.InstructionText.Should().Be("Keep pedaling!");
        }

        [Fact]
        public void GivenOnSegmentStateIsReceivedAndRiderOnFirstSegmentOfRouteButWrongDirection_InstructionIsMakeAUTurn()
        {
            WhenUpdating(new OnSegmentState(1, 2, TrackPoint.Unknown, new Segment(new List<TrackPoint>()), SegmentDirection.BtoA, 0, 0, 0));

            TheCallToAction.InstructionText.Should().Be("Heading the wrong way! Make a U-turn!");
        }

        [Fact]
        public void GivenOnRouteStateAndRouteLockIsLost_InstructionIsMakeUTurn()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1", nextSegmentId: "seg-2"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-2", nextSegmentId: "seg-3"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-3"));
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute, SegmentDirection.AtoB, 0, 0, 0));

            WhenUpdating(new LostRouteLockState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }, plannedRoute, SegmentDirection.AtoB, 0, 0, 0));
            
            TheCallToAction.InstructionText.Should().Be("Heading the wrong way! Make a U-turn to resume the route!");
        }

        [Fact]
        public void GivenRouteLockWasLostAndUserReturnsToRoute_InstructionTextIsCleared()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1", nextSegmentId: "seg-2"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-2", nextSegmentId: "seg-3"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-3"));
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute, SegmentDirection.AtoB, 0, 0, 0));

            WhenUpdating(new LostRouteLockState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }, plannedRoute ,SegmentDirection.AtoB, 0, 0, 0));

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2"}, plannedRoute, SegmentDirection.AtoB, 0, 0, 0));

            _viewModel.CallToAction?.InstructionText.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GivenOnRouteStateIsReceived_NextSegmentIsTheSecondSegmentOfTheRoute()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-1", nextSegmentId: "seg-2"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-2", nextSegmentId: "seg-3"));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId: "seg-3"));
            plannedRoute.EnteredSegment("seg-1");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute, SegmentDirection.AtoB, 0, 0, 0));

            _viewModel.Model.NextSegment!.SegmentId.Should().Be("seg-2");
        }

        private readonly InGameNavigationWindowViewModel _viewModel;

        public WhenUpdatingGameState()
        {
            var segments = new List<Segment>
            {
                new(new List<TrackPoint> { new(1, 2, 3) }){ Id = "seg-1", Name = "Segment 1"},
                new(new List<TrackPoint> { new(1, 2, 3) }){ Id = "seg-2", Name = "Segment 2"},
                new(new List<TrackPoint> { new(1, 2, 3) }){ Id = "seg-3", Name = "Segment 3"},
            };

            var inGameWindowModel = new InGameWindowModel(segments)
            {
                Route = new PlannedRoute
                {
                    World = _world,
                    RouteSegmentSequence =
                    {
                        new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-1"),
                        new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-2"),
                        new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-3")
                    }
                }
            };

            _viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments, new NopGameConnection(), new NopMonitoringEvents(), new StubWindowService());
        }

        private void WhenUpdating(GameState gameState)
        {
            _viewModel.UpdateGameState(gameState);
        }

        private CallToActionViewModel TheCallToAction => _viewModel.CallToAction!;
    }
}
