using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class InGameNavigationWindowViewModel : INotifyPropertyChanged
    {
        private GameState _previousState;
        private int _previousRouteSequenceIndex;
        private readonly List<Segment> _segments;
        private TrackPoint _previousPosition;
        public event PropertyChangedEventHandler PropertyChanged;

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments)
        {
            Model = inGameWindowModel;
            Model.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Model.CurrentSegment):
                    case nameof(Model.NextSegment):
                        OnPropertyChanged(nameof(Model));
                        break;
                }
            };

            _segments = segments;
        }
        
        public InGameWindowModel Model { get; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateGameState(GameState gameState)
        {
            try
            {
                if (gameState is PositionedState positionedState and OnSegmentState)
                {
                    if (_previousPosition != null)
                    {
                        var distanceFromLast = _previousPosition.DistanceTo(positionedState.CurrentPosition);

                        Model.ElapsedDistance += distanceFromLast / 1000;

                        var altitudeDelta = _previousPosition.Altitude - positionedState.CurrentPosition.Altitude;
                        
                        if (altitudeDelta > 0)
                        {
                            Model.ElapsedDescent += -altitudeDelta;
                        }
                        else if (altitudeDelta < 0)
                        {
                            Model.ElapsedAscent += altitudeDelta;
                        }
                    }

                    _previousPosition = positionedState.CurrentPosition;
                    Model.CurrentSegment.PointOnSegment = positionedState.CurrentPosition;
                }

                if (gameState is OnRouteState routeState)
                {
                    if (_previousState is not OnRouteState && 
                        routeState.Route.HasStarted &&
                        routeState.Route.SegmentSequenceIndex == 0)
                    {
                        RouteStarted();
                    }

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
            }
            finally
            {
                _previousState = gameState;
            }
        }

        private void RouteStarted()
        {
            // TODO: decide if we need this
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
            return _segments.SingleOrDefault(s => s.Id == segmentId);
        }
    }
}