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
        private int _previousRouteSequenceIndex;
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
            UpdateUserInGameStatus(gameState);

            try
            {
                if (_previousState is OnRouteState previousRouteState && gameState is LostRouteLockState)
                {
                    if (previousRouteState.Route.NextSegmentId != null)
                    {
                        var expectedSegment = GetSegmentById(previousRouteState.Route.NextSegmentId);
                        Model.InstructionText = $"Try to make a u-turn and head to segment '{expectedSegment.Name}'";
                    }
                    else
                    {
                        Model.InstructionText = $"Try to make a u-turn to return to the route";
                    }

                    Model.LostRouteLock = true;
                }

                if (gameState is OnSegmentState positionedState)
                {
                    Model.CurrentSegment.PointOnSegment = positionedState.CurrentPosition;
                }

                if (gameState is OnSegmentState segmentState)
                {
                    Model.ElapsedAscent = segmentState.ElapsedAscent;
                    Model.ElapsedDescent = segmentState.ElapsedDescent;
                    Model.ElapsedDistance = segmentState.ElapsedDistance;
                }

                if (gameState is OnRouteState routeState)
                {
                    Model.ElapsedAscent = routeState.ElapsedAscent;
                    Model.ElapsedDescent = routeState.ElapsedDescent;
                    Model.ElapsedDistance = routeState.ElapsedDistance;

                    if(_previousRouteSequenceIndex != routeState.Route.SegmentSequenceIndex)
                    {
                        // Moved to next segment on route
                        RouteProgression(routeState.Route.SegmentSequenceIndex);

                        if (Model.Route.IsLoop && Model.Route.RouteSegmentSequence[routeState.Route.SegmentSequenceIndex].Type == SegmentSequenceType.LoopStart)
                        {
                            Model.LoopCount++;
                        }
                    }

                    if (_previousState is OnSegmentState)
                    {
                        // Back on route again
                        RouteProgression(routeState.Route.SegmentSequenceIndex);
                    }

                    _previousRouteSequenceIndex = routeState.Route.SegmentSequenceIndex;

                    if (Model.LostRouteLock)
                    {
                        Model.LostRouteLock = false;
                        Model.InstructionText = string.Empty;
                    }
                }

                if (gameState is CompletedRouteState && !Model.Route.IsLoop)
                {
                    HasRouteFinished = true;
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

        private void UpdateUserInGameStatus(GameState gameState)
        {
            if (gameState is NotLoggedInState || 
                gameState is LoggedInState || 
                gameState is WaitingForConnectionState ||
                gameState is ConnectedToZwiftState)
            {
                Model.UserIsInGame = false;
            }
            else
            {
                Model.UserIsInGame = true;
            }

            if (gameState is InGameState && _previousState is not InGameState)
            {
                Model.UserIsInGame = true;
                Model.WaitingReason = string.Empty;
                Model.InstructionText = string.Empty;
            }
            else if (gameState is ConnectedToZwiftState && _previousState is not ConnectedToZwiftState)
            {
                var sportActivity = GetActivityFromSport();
                Model.UserIsInGame = false;
                Model.WaitingReason = "Connected with Zwift";
                Model.InstructionText = $"Start {sportActivity} in {Model.Route.World.Name} on route:";
            }
            else if (gameState is WaitingForConnectionState && _previousState is InGameState)
            {
                Model.UserIsInGame = false;
                Model.WaitingReason = "Connection with Zwift was lost, waiting for reconnect...";
                Model.InstructionText = string.Empty;
            }
            else if (gameState is WaitingForConnectionState)
            {
                var sportActivity = GetActivityFromSport();
                Model.UserIsInGame = false;
                Model.WaitingReason = "Waiting for Zwift...";
                Model.InstructionText = $"Start Zwift and start {sportActivity} in {Model.Route.World.Name} on route:";
            }
            else if (gameState is OnRouteState)
            {
                Model.WaitingReason = string.Empty;
                Model.InstructionText = string.Empty;
            }
            else if (gameState is ErrorState)
            {
                Model.UserIsInGame = false;
                Model.WaitingReason = "Oops! Something went wrong...";
                Model.InstructionText = "Please report a bug on Github";
            }
        }

        private string GetActivityFromSport()
        {
            return Model.Route.Sport switch
            {
                SportType.Cycling => "cycling",
                SportType.Running => "running",
                _ => "cycling"
            };
        }

        private void RouteProgression(int segmentSequenceIndex)
        {
            // Set CurrentSegment and NextSegment accordingly
            Model.CurrentSegment = SegmentSequenceModelFromIndex(segmentSequenceIndex);

            if (segmentSequenceIndex < Model.Route.RouteSegmentSequence.Count - 1)
            {
                Model.NextSegment = SegmentSequenceModelFromIndex(segmentSequenceIndex + 1);
            }
            else
            {
                Model.NextSegment = null;
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