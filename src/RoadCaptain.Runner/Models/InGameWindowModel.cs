using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.Runner.Annotations;

namespace RoadCaptain.Runner.Models
{
    public class InGameWindowModel : INotifyPropertyChanged
    {
        private string _windowTitle = "RoadCaptain";
        private double _elapsedDistance;
        private double _elapsedAscent;
        private double _elapsedDescent;
        private SegmentSequenceModel _currentSegment;
        private SegmentSequenceModel _nextSegment;
        private readonly List<Segment> _segments;
        private PlannedRoute _route;

        public InGameWindowModel(List<Segment> segments)
        {
            _segments = segments;
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (value == _windowTitle)
                {
                    return;
                }

                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public PlannedRoute Route
        {
            get => _route;
            private set
            {
                if (Equals(value, _route)) return;
                _route = value;
                OnPropertyChanged();
            }
        }

        public double ElapsedDistance
        {
            get => _elapsedDistance;
            set
            {
                if (value.Equals(_elapsedDistance)) return;
                _elapsedDistance = value;
                OnPropertyChanged();
            }
        }

        public double ElapsedAscent
        {
            get => _elapsedAscent;
            set
            {
                if (value.Equals(_elapsedAscent)) return;
                _elapsedAscent = value;
                OnPropertyChanged();
            }
        }

        public double ElapsedDescent
        {
            get => _elapsedDescent;
            set
            {
                if (value.Equals(_elapsedDescent)) return;
                _elapsedDescent = value;
                OnPropertyChanged();
            }
        }

        public SegmentSequenceModel CurrentSegment
        {
            get => _currentSegment;
            set
            {
                if (Equals(value, _currentSegment)) return;
                _currentSegment = value;
                OnPropertyChanged();
            }
        }

        public SegmentSequenceModel NextSegment
        {
            get => _nextSegment;
            set
            {
                if (Equals(value, _nextSegment)) return;
                _nextSegment = value;
                OnPropertyChanged();
            }
        }

        public string ZwiftUsername { get; set; }
        public string ZwiftPassword { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitializeRoute(PlannedRoute route)
        {
            Route = route;

            var currentSegmentSequence = route.RouteSegmentSequence[route.SegmentSequenceIndex];
            CurrentSegment = new SegmentSequenceModel(
                currentSegmentSequence, 
                GetSegmentById(currentSegmentSequence.SegmentId), 
                route.SegmentSequenceIndex);

            if (route.SegmentSequenceIndex < route.RouteSegmentSequence.Count - 1)
            {
                var nextSegmentSequence = route.RouteSegmentSequence[route.SegmentSequenceIndex + 1];
                NextSegment = new SegmentSequenceModel(
                    nextSegmentSequence,
                    GetSegmentById(currentSegmentSequence.SegmentId), 
                    route.SegmentSequenceIndex + 1);
            }
            else
            {
                NextSegment = null;
            }

            ElapsedAscent = 0;
            ElapsedDescent = 0;
            ElapsedDistance = 0;
        }

        private Segment GetSegmentById(string segmentId)
        {
            return _segments.SingleOrDefault(s => s.Id == segmentId);
        }
    }
}