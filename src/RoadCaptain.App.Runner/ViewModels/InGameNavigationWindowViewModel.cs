// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.ViewModels;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class InGameNavigationWindowViewModel : ViewModelBase
    {
        private GameState? _previousState;
        private readonly List<Segment> _segments;
        private bool _hasRouteFinished;
        private readonly IZwiftGameConnection _gameConnection;
        private CallToActionViewModel? _callToAction;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IWindowService _windowService;

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments, IZwiftGameConnection gameConnection, MonitoringEvents monitoringEvents, IWindowService windowService)
        {
            Model = inGameWindowModel;
            Model.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Model.CurrentSegment):
                    case nameof(Model.NextSegment):
                        this.RaisePropertyChanged(nameof(Model));
                        break;
                }
            };

            _segments = segments;
            _gameConnection = gameConnection;
            _monitoringEvents = monitoringEvents;
            _windowService = windowService;

            EndActivityCommand = new AsyncRelayCommand(
                _ => EndActivity(),
                _ => true);

            ToggleElevationProfileCommand = new AsyncRelayCommand(
                show => ToggleElevationProfile(show as bool?),
                _ => true);
        }

        public InGameWindowModel Model { get; }
        public ICommand EndActivityCommand { get; }
        public ICommand ToggleElevationProfileCommand { get; }

        public void UpdateGameState(GameState gameState)
        {
            if (_previousState == null || gameState.GetType() != _previousState.GetType())
            {
                _monitoringEvents.Information($"ViewModel state transition from {(_previousState == null ? "none" : _previousState.GetType().Name)} to {gameState.GetType().Name}");
            }

            try
            {
                switch (gameState)
                {
                    case LoggedInState:
                    case ReadyToGoState:
                        if (Model.Route?.World != null)
                        {
                            CallToAction = new CallToActionViewModel(
                                "Waiting for Zwift...",
                                $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
                        }

                        break;
                    case ConnectedToZwiftState:
                        if (Model.Route?.World != null)
                        {
                            CallToAction = new CallToActionViewModel(
                                "Connected with Zwift",
                                $"Start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
                        }
                        break;
                    case WaitingForConnectionState when GameState.IsInGame(_previousState):
                        CallToAction = new CallToActionViewModel(
                                "Connection with Zwift was lost, waiting for reconnect...",
                                string.Empty);
                        break;
                    case WaitingForConnectionState:
                        if (Model.Route?.World != null)
                        {
                            CallToAction = new CallToActionViewModel(
                                "Waiting for Zwift...",
                                $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
                        }
                        break;
                    case InGameState:
                        CallToAction = new CallToActionViewModel(
                                "Entered the game",
                                "Start pedaling!");
                        break;
                    case PositionedState:
                        CallToAction = new CallToActionViewModel(
                            "Riding to start of route",
                            "Keep pedaling!");
                        break;
                    case OnSegmentState segmentState:
                        if (Model.Route?.World != null)
                        {
                            if (!Model.Route.HasStarted &&
                                Model.Route.RouteSegmentSequence[0].Direction != segmentState.Direction &&
                                segmentState.Direction != SegmentDirection.Unknown)
                            {
                                CallToAction = new CallToActionViewModel(
                                    "Riding to start of route",
                                    "Heading the wrong way! Make a U-turn!");
                            }
                            else
                            {
                                CallToAction = new CallToActionViewModel(
                                    "Riding to start of route",
                                    "Keep pedaling!");
                            }
                        }
                        break;
                    case IncorrectConnectionSecretState:
                        CallToAction = new CallToActionViewModel(
                            "Zwift connection failed",
                            "Retrying connection...",
                            "#FF0000");
                        break;
                    case ErrorState errorState:
                        CallToAction = new CallToActionViewModel(
                                "Oops! Something went wrong...",
                                $"{errorState.Message}.\nPlease report a bug on Github",
                                "#FF0000");
                        break;
                    case LostRouteLockState lostRouteState:
                        var instructionText = "Try to make a u-turn to return to the route";
                        // If CurrentSegmentSequence is null here we deserve to blow up
                        if (lostRouteState.Route.CurrentSegmentSequence!.Direction != lostRouteState.Direction)
                        {
                            instructionText = "Heading the wrong way! Make a U-turn to resume the route!";
                        }
                        else if (lostRouteState.Route.NextSegmentId != null)
                        {
                            var expectedSegment = GetSegmentById(lostRouteState.Route.NextSegmentId);
                            instructionText =
                                $"Try to head to segment '{expectedSegment.Name}'";
                        }

                        CallToAction = new CallToActionViewModel(
                                "Lost route lock",
                                instructionText);

                        break;
                    case OnRouteState routeState:
                        {
                            if (Model.CurrentSegment?.SegmentId != routeState.Route.CurrentSegmentId)
                            {
                                // Moved to next segment on route
                                UpdateRouteModel(routeState.Route);
                            }

                            if (Model.CurrentSegment != null)
                            {
                                Model.CurrentSegment.PointOnSegment = routeState.CurrentPosition;
                            }

                            Model.ElapsedAscent = routeState.ElapsedAscent;
                            Model.ElapsedDescent = routeState.ElapsedDescent;
                            Model.ElapsedDistance = routeState.ElapsedDistance;
                            Model.CurrentSegmentIndex = routeState.Route.SegmentSequenceIndex + 1;

                            CallToAction = null;

                            break;
                        }
                    case UpcomingTurnState upcomingTurnState:
                        {
                            if (Model.CurrentSegment?.SegmentId != upcomingTurnState.Route.CurrentSegmentId)
                            {
                                // Moved to next segment on route
                                UpdateRouteModel(upcomingTurnState.Route);
                            }

                            Model.CurrentSegment!.PointOnSegment = upcomingTurnState.CurrentPosition;
                            Model.ElapsedAscent = upcomingTurnState.ElapsedAscent;
                            Model.ElapsedDescent = upcomingTurnState.ElapsedDescent;
                            Model.ElapsedDistance = upcomingTurnState.ElapsedDistance;
                            Model.CurrentSegmentIndex = upcomingTurnState.Route.SegmentSequenceIndex + 1;

                            CallToAction = null;

                            break;
                        }
                    case OnLoopState loopState:
                    {
                        if (Model.CurrentSegment?.SegmentId != loopState.Route.CurrentSegmentId)
                        {
                            // Moved to next segment on route
                            UpdateRouteModel(loopState.Route);
                        }

                        if (Model.CurrentSegment != null)
                        {
                            Model.CurrentSegment.PointOnSegment = loopState.CurrentPosition;
                        }

                        Model.ElapsedAscent = loopState.ElapsedAscent;
                        Model.ElapsedDescent = loopState.ElapsedDescent;
                        Model.ElapsedDistance = loopState.ElapsedDistance;
                        Model.CurrentSegmentIndex = loopState.Route.SegmentSequenceIndex + 1;

                        CallToAction = null;

                        break;
                    }
                    case CompletedRouteState { Route.IsLoop: false } completedRoute:
                        {
                            HasRouteFinished = true;

                            CallToAction = null;

                            if (Model.CurrentSegment?.SegmentId != completedRoute.Route.CurrentSegmentId)
                            {
                                // Moved to next segment on route
                                UpdateRouteModel(completedRoute.Route);
                            }

                            break;
                        }
                }
            }
            finally
            {
                _previousState = gameState;
            }
        }

        public bool HasRouteFinished
        {
            get => _hasRouteFinished;
            set
            {
                if (value == _hasRouteFinished) return;
                _hasRouteFinished = value;
                this.RaisePropertyChanged();
            }
        }

        public ulong LastSequenceNumber { get; set; }

        public CallToActionViewModel? CallToAction
        {
            get => _callToAction;
            set
            {
                if (value == _callToAction) return;
                _callToAction = value;
                this.RaisePropertyChanged();
            }
        }

        private string GetActivityFromSport()
        {
            return Model.Route?.Sport switch
            {
                SportType.Cycling => "cycling",
                SportType.Running => "running",
                _ => "cycling"
            };
        }

        private void UpdateRouteModel(PlannedRoute plannedRoute)
        {
            // Set CurrentSegment and NextSegment accordingly
            if (plannedRoute.CurrentSegmentSequence != null)
            {
                Model.CurrentSegment = new SegmentSequenceModel(plannedRoute.CurrentSegmentSequence, GetSegmentById(plannedRoute.CurrentSegmentSequence.SegmentId));
            }
            else
            {
                Model.CurrentSegment = null;
            }

            if (plannedRoute.NextSegmentSequence != null)
            {
                Model.NextSegment = new SegmentSequenceModel(plannedRoute.NextSegmentSequence, GetSegmentById(plannedRoute.NextSegmentSequence.SegmentId));
            }
            else
            {
                Model.NextSegment = null;
            }

            if (plannedRoute.IsLoop)
            {
                Model.LoopText = plannedRoute.OnLeadIn ? "Lead-in to loop" : $"On loop: {plannedRoute.LoopCount}";
                Model.IsOnLoop = plannedRoute.IsOnLoop;
            }
            else
            {
                Model.LoopText = string.Empty;
            }
        }

        private Segment GetSegmentById(string segmentId)
        {
            var segment = _segments.SingleOrDefault(s => s.Id == segmentId);

            if (segment == null)
            {
                throw new Exception($"Segment '{segmentId}' was not found");
            }

            return segment;
        }

        private Task<CommandResult> EndActivity()
        {
            if (GameState.IsInGame(_previousState))
            {
                _gameConnection.EndActivity(LastSequenceNumber, "RoadCaptain: " + Model.Route?.Name,
                    _previousState?.RiderId ?? 0);
            }
            else
            {
                _windowService.ShowMainWindow();
            }

            return Task.FromResult(CommandResult.Success());
        }

        private Task<CommandResult> ToggleElevationProfile(bool? show)
        {
            _windowService.ToggleElevationProfile(Model.Route, show);

            return Task.FromResult(CommandResult.Success());
        }
    }
}
