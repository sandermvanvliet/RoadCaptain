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

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments, IZwiftGameConnection gameConnection)
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

            EndActivityCommand = new AsyncRelayCommand(
                _ => EndActivity(),
                _ => true);
        }

        public InGameWindowModel Model { get; }
        public ICommand EndActivityCommand { get; }

        public void UpdateGameState(GameState gameState)
        {
            if (GameState.IsInGame(gameState))
            {
                Model.UserIsInGame = true;
                Model.WaitingReason = string.Empty;
                Model.InstructionText = string.Empty;
            }
            else 
            {
                Model.UserIsInGame = false;
                Model.WaitingReason = string.Empty;
                Model.InstructionText = string.Empty;
            }
            
            try
            {
                switch (gameState)
                {
                    case LoggedInState:
                        Model.UserIsInGame = false;
                        Model.WaitingReason = "Waiting for Zwift...";
                        Model.InstructionText = $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route:";
                        break;
                    case ConnectedToZwiftState:
                        Model.UserIsInGame = false;
                        Model.WaitingReason = "Connected with Zwift";
                        Model.InstructionText = $"Start {GetActivityFromSport()} in {Model.Route.World.Name} on route:";
                        break;
                    case WaitingForConnectionState when GameState.IsInGame(_previousState):
                        Model.UserIsInGame = false;
                        Model.WaitingReason = "Connection with Zwift was lost, waiting for reconnect...";
                        Model.InstructionText = string.Empty;
                        break;
                    case WaitingForConnectionState:
                        Model.UserIsInGame = false;
                        Model.WaitingReason = "Waiting for Zwift...";
                        Model.InstructionText = $"Start Zwift and start {GetActivityFromSport()} in {Model.Route.World.Name} on route:";
                        break;
                    case ErrorState:
                        Model.UserIsInGame = false;
                        Model.WaitingReason = "Oops! Something went wrong...";
                        Model.InstructionText = "Please report a bug on Github";
                        break;
                    case LostRouteLockState lostRouteState:
                    {
                        if (lostRouteState.Route.NextSegmentId != null)
                        {
                            var expectedSegment = GetSegmentById(lostRouteState.Route.NextSegmentId);
                            Model.InstructionText = $"Try to make a u-turn and head to segment '{expectedSegment.Name}'";
                        }
                        else
                        {
                            Model.InstructionText = $"Try to make a u-turn to return to the route";
                        }

                        Model.LostRouteLock = true;
                        break;
                    }
                    case OnRouteState routeState:
                    {
                        if (Model.CurrentSegment?.SegmentId != routeState.Route.CurrentSegmentId)
                        {
                            // Moved to next segment on route
                            UpdateRouteModel(routeState.Route);
                        }

                        Model.CurrentSegment!.PointOnSegment = routeState.CurrentPosition;
                        Model.ElapsedAscent = routeState.ElapsedAscent;
                        Model.ElapsedDescent = routeState.ElapsedDescent;
                        Model.ElapsedDistance = routeState.ElapsedDistance;
                            
                        Model.LostRouteLock = false;
                        Model.InstructionText = string.Empty;

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
                            
                        Model.LostRouteLock = false;
                        Model.InstructionText = string.Empty;

                        break;
                    }
                    case CompletedRouteState completedRoute when !Model.Route.IsLoop:
                    {
                        HasRouteFinished = true;

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

        private string GetActivityFromSport()
        {
            return Model.Route.Sport switch
            {
                SportType.Cycling => "cycling",
                SportType.Running => "running",
                _ => "cycling"
            };
        }

        private void UpdateRouteModel(PlannedRoute plannedRoute)
        {
            // Set CurrentSegment and NextSegment accordingly
            Model.CurrentSegment = SegmentSequenceModelFromIndex(plannedRoute.SegmentSequenceIndex);

            if (plannedRoute.SegmentSequenceIndex < plannedRoute.RouteSegmentSequence.Count - 1)
            {
                Model.NextSegment = SegmentSequenceModelFromIndex(plannedRoute.SegmentSequenceIndex + 1);
            }
            else
            {
                Model.NextSegment = null;
            }
            
            if (plannedRoute.IsLoop && 
                plannedRoute.RouteSegmentSequence[plannedRoute.SegmentSequenceIndex].Type == SegmentSequenceType.LoopStart)
            {
                Model.LoopCount++;
            }
        }

        private SegmentSequenceModel SegmentSequenceModelFromIndex(int index)
        {
            var currentSegmentSequence = Model.Route.RouteSegmentSequence[index];
            return new SegmentSequenceModel(
                currentSegmentSequence,
                GetSegmentById(currentSegmentSequence.SegmentId),
                index);
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