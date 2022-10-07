using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Shared.Commands;
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

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments, IZwiftGameConnection gameConnection, MonitoringEvents monitoringEvents)
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

            EndActivityCommand = new AsyncRelayCommand(
                _ => EndActivity(),
                _ => true);
        }

        public InGameWindowModel Model { get; }
        public ICommand EndActivityCommand { get; }

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
                        CallToAction = new CallToActionViewModel(
                                "Waiting for Zwift...",
                                $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
                        break;
                    case ConnectedToZwiftState:
                        CallToAction = new CallToActionViewModel(
                                "Connected with Zwift",
                                $"Start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
                        break;
                    case WaitingForConnectionState when GameState.IsInGame(_previousState):
                        CallToAction = new CallToActionViewModel(
                                "Connection with Zwift was lost, waiting for reconnect...",
                                string.Empty);
                        break;
                    case WaitingForConnectionState:
                        CallToAction = new CallToActionViewModel(
                                "Waiting for Zwift...",
                                $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route: {Model.Route.ZwiftRouteName}");
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
                        if (!Model.Route.HasStarted && Model.Route.RouteSegmentSequence[0].Direction != segmentState.Direction)
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
                        if (lostRouteState.Route.NextSegmentId != null)
                        {
                            var expectedSegment = GetSegmentById(lostRouteState.Route.NextSegmentId);
                            instructionText =
                                $"Try to make a u-turn and head to segment '{expectedSegment.Name}'";
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

                            CallToAction = null;

                            break;
                        }
                    case CompletedRouteState completedRoute when !completedRoute.Route.IsLoop:
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
            _gameConnection.EndActivity(LastSequenceNumber, "RoadCaptain: " + Model.Route.Name, _previousState?.RiderId ?? 0);

            return Task.FromResult(CommandResult.Success());
        }
    }
}