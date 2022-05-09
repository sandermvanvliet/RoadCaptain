using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.GameStates;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class InGameNavigationWindowViewModel : ViewModelBase
    {
        private GameState? _previousState;
        private int _previousRouteSequenceIndex;
        private readonly List<Segment> _segments;
        private TrackPoint? _previousPosition;
        private bool _hasRouteFinished;

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments)
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
        }
        
        public InGameWindowModel Model { get; }

        public void UpdateGameState(GameState gameState)
        {
            UpdateUserInGameStatus(gameState);

            try
            {
                if (gameState is PositionedState positionedState and OnSegmentState)
                {
                    if (_previousPosition != null)
                    {
                        var distanceFromLast = _previousPosition.DistanceTo(positionedState.CurrentPosition);

                        Model.ElapsedDistance += distanceFromLast / 1000;

                        if (_previousPosition.Altitude < positionedState.CurrentPosition.Altitude)
                        {
                            Model.ElapsedAscent += positionedState.CurrentPosition.Altitude - _previousPosition.Altitude;
                        }
                        else if (_previousPosition.Altitude > positionedState.CurrentPosition.Altitude)
                        {
                            Model.ElapsedDescent += _previousPosition.Altitude - positionedState.CurrentPosition.Altitude;
                        }
                    }

                    _previousPosition = positionedState.CurrentPosition;
                    Model.CurrentSegment.PointOnSegment = positionedState.CurrentPosition;
                }

                if (gameState is OnRouteState routeState)
                {
                    if(_previousRouteSequenceIndex != routeState.Route.SegmentSequenceIndex)
                    {
                        // Moved to next segment on route
                        RouteProgression(routeState.Route.SegmentSequenceIndex);
                    }

                    if (_previousState is OnSegmentState and not OnRouteState)
                    {
                        // Back on route again
                        RouteProgression(routeState.Route.SegmentSequenceIndex);
                    }

                    _previousRouteSequenceIndex = routeState.Route.SegmentSequenceIndex;
                }

                if (gameState is CompletedRouteState)
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

        private void UpdateUserInGameStatus(GameState gameState)
        {
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
                Model.InstructionText = $"Start Zwift and start {sportActivity} in {Model.Route.World.Name} on route:";
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
    }
}