// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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
        private double _totalAscent;
        private double _totalDescent;
        private double _totalDistance;
        private bool _userIsInGame;
        private string _waitingReason = "Waiting for Zwift connection...";
        private string _instructionText;

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

        public bool UserIsInGame
        {
            get => _userIsInGame;
            set
            {
                if (value == _userIsInGame) return;
                _userIsInGame = value;
                OnPropertyChanged();
            }
        }

        public PlannedRoute Route
        {
            get => _route;
            set
            {
                if (Equals(value, _route)) return;
                _route = value;
                InitializeRoute(value);
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

        public double TotalDistance
        {
            get => _totalDistance;
            set
            {
                if (value.Equals(_totalDistance)) return;
                _totalDistance = value;
                OnPropertyChanged();
            }
        }

        public double TotalAscent
        {
            get => _totalAscent;
            set
            {
                if (value.Equals(_totalAscent)) return;
                _totalAscent = value;
                OnPropertyChanged();
            }
        }

        public double TotalDescent
        {
            get => _totalDescent;
            set
            {
                if (value.Equals(_totalDescent)) return;
                _totalDescent = value;
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

        public string WaitingReason
        {
            get => _waitingReason;
            set
            {
                if (value == _waitingReason) return;
                _waitingReason = value;
                OnPropertyChanged();
            }
        }

        public string InstructionText
        {
            get => _instructionText;
            set
            {
                if (value == _instructionText) return;
                _instructionText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeRoute(PlannedRoute route)
        {
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

            CalculateTotalAscentAndDescent(route);

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
            return _segments.SingleOrDefault(s => s.Id == segmentId);
        }
    }
}
