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
        public event PropertyChangedEventHandler PropertyChanged;

        public InGameNavigationWindowViewModel(InGameWindowModel inGameWindowModel, List<Segment> segments)
        {
            Model = inGameWindowModel;
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

                    if (_previousState is OnSegmentState)
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