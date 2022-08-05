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
        private World World = new World { Id = "testworld", Name = "TestWorld" };

        [Fact]
        public void GivenWaitingForConnectionState_UserInGameIsFalse()
        {
            WhenUpdating(new WaitingForConnectionState());

            _viewModel.Model.UserIsInGame.Should().BeFalse();
        }

        [Fact]
        public void GivenWaitingForConnectionState_WaitReasonIsWaitingForConnection()
        {
            WhenUpdating(new WaitingForConnectionState());

            _viewModel
                .Model
                .WaitingReason
                .Should()
                .Be("Waiting for Zwift...");
        }

        [Fact]
        public void GivenWaitingForConnectionState_InstructionIsStartZwift()
        {
            WhenUpdating(new WaitingForConnectionState());

            _viewModel
                .Model
                .InstructionText
                .Should()
                .Be($"Start Zwift and start cycling in {World.Name} on route:");
        }

        [Fact]
        public void GivenWaitingForConnectionAndInGameStateIsReceived_UserInGameIsTrue()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new InGameState(1, 2));

            _viewModel.Model.UserIsInGame.Should().BeTrue();
        }

        [Fact]
        public void GivenWaitingForConnectionAndInGameStateIsReceived_WaitReasonIsEmptyString()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new InGameState(1, 2));

            _viewModel
                .Model
                .WaitingReason
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenWaitingForConnectionAndInGameStateIsReceived_InstructionIsEmptyString()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new InGameState(1, 2));

            _viewModel
                .Model
                .InstructionText
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenInGameAndWaitingForConnectionStateIsReceived_UserInGameIsFalse()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new WaitingForConnectionState());

            _viewModel.Model.UserIsInGame.Should().BeFalse();
        }

        [Fact]
        public void GivenInGameAndWaitingForConnectionStateIsReceived_WaitReasonIsConnectionLost()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new WaitingForConnectionState());

            _viewModel
                .Model
                .WaitingReason
                .Should()
                .Be("Connection with Zwift was lost, waiting for reconnect...");
        }

        [Fact]
        public void GivenInGameAndWaitingForConnectionStateIsReceived_InstructionIsEmpty()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new WaitingForConnectionState());

            _viewModel
                .Model
                .InstructionText
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenInGameAndConnectedStateIsReceived_UserInGameIsFalse()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel.Model.UserIsInGame.Should().BeFalse();
        }

        [Fact]
        public void GivenInGameAndConnectedStateIsReceived_WaitReasonIsConnectedWithZwift()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel
                .Model
                .WaitingReason
                .Should()
                .Be("Connected with Zwift");
        }

        [Fact]
        public void GivenInGameAndConnectedStateIsReceived_InstructionIsStartRiding()
        {
            WhenUpdating(new InGameState(1, 2));

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel
                .Model
                .InstructionText
                .Should()
                .Be($"Start cycling in {World.Name} on route:");
        }

        [Fact]
        public void GivenWaitingForConnectionAndConnectedStateIsReceived_UserInGameIsFalse()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel.Model.UserIsInGame.Should().BeFalse();
        }

        [Fact]
        public void GivenWaitingForConnectionAndConnectedStateIsReceived_WaitReasonIsConnectedWithZwift()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel
                .Model
                .WaitingReason
                .Should()
                .Be("Connected with Zwift");
        }

        [Fact]
        public void GivenWaitingForConnectionAndConnectedStateIsReceived_InstructionIsStartRiding()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new ConnectedToZwiftState());

            _viewModel
                .Model
                .InstructionText
                .Should()
                .Be($"Start cycling in {World.Name} on route:");
        }

        [Fact]
        public void GivenWaitingForConnectionAndOnRouteStateIsReceived_UserInGameIsTrue()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()), new PlannedRoute()));

            _viewModel.Model.UserIsInGame.Should().BeTrue();
        }

        [Fact]
        public void GivenWaitingForConnectionAndOnRouteStateIsReceived_WaitReasonIsEmpty()
        {
            WhenUpdating(new WaitingForConnectionState());

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()), new PlannedRoute()));

            _viewModel.Model.WaitingReason.Should().BeEmpty();
        }

        [Fact]
        public void GivenErrorStateIsReceived_UserInGameIsFalse()
        {
            WhenUpdating(new ErrorState(new Exception("BANG")));

            _viewModel.Model.UserIsInGame.Should().BeFalse();
        }

        [Fact]
        public void GivenErrorStateIsReceived_WaitReasonIsOopsSomethingWentWrong()
        {
            WhenUpdating(new ErrorState(new Exception("BANG")));

            _viewModel.Model.WaitingReason.Should().Be("Oops! Something went wrong...");
        }

        [Fact]
        public void GivenErrorStateIsReceived_InstructionIsPleaseReportBug()
        {
            WhenUpdating(new ErrorState(new Exception("BANG")));

            _viewModel.Model.InstructionText.Should().Be("Please report a bug on Github");
        }

        [Fact]
        public void GivenOnRouteStateAndRouteLockIsLost_LostRouteLockIsTrue()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", });
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute));
            
            WhenUpdating(new LostRouteLockState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }, SegmentDirection.AtoB, plannedRoute, 0, 0, 0));

            _viewModel.Model.LostRouteLock.Should().BeTrue();
        }

        [Fact]
        public void GivenOnRouteStateAndRouteLockIsLost_InstructionIsMakeUTurn()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", });
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute));

            WhenUpdating(new LostRouteLockState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }, SegmentDirection.AtoB, plannedRoute, 0, 0, 0));
            
            _viewModel.Model.InstructionText.Should().Be("Try to make a u-turn and head to segment 'Segment 3'");
        }

        [Fact]
        public void GivenRouteLockWasLostAndUserReturnsToRoute_LostRouteLockIsFalse()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", });
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute));
            
            WhenUpdating(new LostRouteLockState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }, SegmentDirection.AtoB, plannedRoute, 0, 0, 0));

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2"}, plannedRoute));

            _viewModel.Model.LostRouteLock.Should().BeFalse();
        }

        [Fact]
        public void GivenRouteLockWasLostAndUserReturnsToRoute_InstructionTextIsCleared()
        {
            var plannedRoute = new PlannedRoute();
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-1", NextSegmentId = "seg-2"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-2", NextSegmentId = "seg-3"});
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence { SegmentId = "seg-3", });
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-1"}, plannedRoute));

            WhenUpdating(new OnSegmentState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2" }));

            WhenUpdating(new OnRouteState(1, 2, new TrackPoint(1, 2, 3), new Segment(new List<TrackPoint>()) { Id = "seg-2"}, plannedRoute));

            _viewModel.Model.InstructionText.Should().BeEmpty();
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
                    World = World,
                    RouteSegmentSequence =
                    {
                        new SegmentSequence
                        {
                            Direction = SegmentDirection.AtoB,
                            SegmentId = "seg-1"
                        },
                        new SegmentSequence
                        {
                            Direction = SegmentDirection.AtoB,
                            SegmentId = "seg-2"
                        },
                        new SegmentSequence
                        {
                            Direction = SegmentDirection.AtoB,
                            SegmentId = "seg-3"
                        }
                    }
                }
            };

            _viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments, null);
        }


        private void WhenUpdating(GameState gameState)
        {
            _viewModel.UpdateGameState(gameState);
        }
    }
}