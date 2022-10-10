using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using RoadCaptain.App.Runner.ViewModels;

namespace RoadCaptain.App.Runner.Models
{
    public class InGameWindowModel : ViewModelBase
    {
        private string _windowTitle = "RoadCaptain";
        private double _elapsedDistance;
        private double _elapsedAscent;
        private double _elapsedDescent;
        private SegmentSequenceModel? _currentSegment;
        private SegmentSequenceModel? _nextSegment;
        private readonly List<Segment> _segments;
        private PlannedRoute? _route;
        private double _totalAscent;
        private double _totalDescent;
        private double _totalDistance;
        private string _loopText = string.Empty;
        private int _currentSegmentIndex;

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
                this.RaisePropertyChanged();
            }
        }


        public PlannedRoute? Route
        {
            get => _route;
            set
            {
                if (Equals(value, _route)) return;
                _route = value;
                if (_route != null)
                {
                    InitializeRoute(_route);
                }
                else
                {
                    ClearRoute();
                }
                this.RaisePropertyChanged();
            }
        }

        public double ElapsedDistance
        {
            get => _elapsedDistance;
            set
            {
                if (value.Equals(_elapsedDistance)) return;
                _elapsedDistance = value;
                this.RaisePropertyChanged();
            }
        }

        public double ElapsedAscent
        {
            get => _elapsedAscent;
            set
            {
                if (value.Equals(_elapsedAscent)) return;
                _elapsedAscent = value;
                this.RaisePropertyChanged();
            }
        }

        public double ElapsedDescent
        {
            get => _elapsedDescent;
            set
            {
                if (value.Equals(_elapsedDescent)) return;
                _elapsedDescent = value;
                this.RaisePropertyChanged();
            }
        }

        public double TotalDistance
        {
            get => _totalDistance;
            set
            {
                if (value.Equals(_totalDistance)) return;
                _totalDistance = value;
                this.RaisePropertyChanged();
            }
        }

        public double TotalAscent
        {
            get => _totalAscent;
            set
            {
                if (value.Equals(_totalAscent)) return;
                _totalAscent = value;
                this.RaisePropertyChanged();
            }
        }

        public double TotalDescent
        {
            get => _totalDescent;
            set
            {
                if (value.Equals(_totalDescent)) return;
                _totalDescent = value;
                this.RaisePropertyChanged();
            }
        }

        public SegmentSequenceModel? CurrentSegment
        {
            get => _currentSegment;
            set
            {
                if (Equals(value, _currentSegment)) return;
                _currentSegment = value;
                this.RaisePropertyChanged();
            }
        }

        public SegmentSequenceModel? NextSegment
        {
            get => _nextSegment;
            set
            {
                if (Equals(value, _nextSegment)) return;
                _nextSegment = value;
                this.RaisePropertyChanged();
            }
        }

        public string LoopText
        {
            get => _loopText;
            set
            {
                if(value == _loopText) return;
                _loopText = value;
                this.RaisePropertyChanged();
            }
        }

        public int CurrentSegmentIndex
        {
            get => _currentSegmentIndex;
            set
            {
                if (value == _currentSegmentIndex) return;
                _currentSegmentIndex = value;
                this.RaisePropertyChanged();
            }
        }

        public int SegmentCount => Route?.RouteSegmentSequence.Count ?? 0;

        private void InitializeRoute(PlannedRoute route)
        {
            if (route.CurrentSegmentSequence != null)
            {
                CurrentSegment = new SegmentSequenceModel(
                route.CurrentSegmentSequence,
                GetSegmentById(route.CurrentSegmentSequence.SegmentId));
            }
            else
            {
                CurrentSegment = null;
            }

            if (route.NextSegmentSequence != null)
            {
                NextSegment = new SegmentSequenceModel(
                    route.NextSegmentSequence,
                    GetSegmentById(route.NextSegmentSequence.SegmentId));
            }
            else
            {
                NextSegment = null;
            }

            CalculateTotalAscentAndDescent(route);

            ElapsedAscent = 0;
            ElapsedDescent = 0;
            ElapsedDistance = 0;
        }

        private void ClearRoute()
        {
            CurrentSegment = null;
            NextSegment = null;

            ElapsedAscent = 0;
            ElapsedDescent = 0;
            ElapsedDistance = 0;
        }

        private void CalculateTotalAscentAndDescent(PlannedRoute route)
        {
            double totalAscent = 0;
            double totalDescent = 0;
            double totalDistance = 0;

            foreach (var sequence in route.RouteSegmentSequence)
            {
                var segment = GetSegmentById(sequence.SegmentId);

                if (sequence.Direction == SegmentDirection.AtoB)
                {
                    totalAscent += segment.Ascent;
                    totalDescent += segment.Descent;
                }
                else
                {
                    totalAscent += segment.Descent;
                    totalDescent += segment.Ascent;
                }

                totalDistance += segment.Distance;
            }

            TotalDistance = Math.Round(totalDistance / 1000, 1);
            TotalAscent = totalAscent;
            TotalDescent = totalDescent;
        }

        private Segment GetSegmentById(string segmentId)
        {
            var segment = _segments.SingleOrDefault(s => s.Id == segmentId);

            if (segment == null)
            {
                throw new Exception($"Could not find segment with id '{segmentId}'");
            }

            return segment;
        }
    }
}