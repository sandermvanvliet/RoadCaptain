// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.ViewModels;

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
        private bool _isOnLoop;
        private double _loopDistance;

        public InGameWindowModel(List<Segment> segments)
        {
            _segments = segments;
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }


        public PlannedRoute? Route
        {
            get => _route;
            set
            {
                SetProperty(ref _route, value);
                
                if (_route != null)
                {
                    InitializeRoute(_route);
                }
                else
                {
                    ClearRoute();
                }
            }
        }

        public double ElapsedDistance
        {
            get => _elapsedDistance;
            set => SetProperty(ref _elapsedDistance, value);
        }

        public double ElapsedAscent
        {
            get => _elapsedAscent;
            set => SetProperty(ref _elapsedAscent, value);
        }

        public double ElapsedDescent
        {
            get => _elapsedDescent;
            set => SetProperty(ref _elapsedDescent, value);
        }

        public double TotalDistance
        {
            get => IsOnLoop ? _loopDistance : _totalDistance;
            set => SetProperty(ref _totalDistance, value);
        }

        public double TotalAscent
        {
            get => _totalAscent;
            set => SetProperty(ref _totalAscent, value);
        }

        public double TotalDescent
        {
            get => _totalDescent;
            set => SetProperty(ref _totalDescent, value);
        }

        public SegmentSequenceModel? CurrentSegment
        {
            get => _currentSegment;
            set => SetProperty(ref _currentSegment, value);
        }

        public SegmentSequenceModel? NextSegment
        {
            get => _nextSegment;
            set => SetProperty(ref _nextSegment, value);
        }

        public string LoopText
        {
            get => _loopText;
            set => SetProperty(ref _loopText, value);
        }

        public int CurrentSegmentIndex
        {
            get => _currentSegmentIndex;
            set => SetProperty(ref _currentSegmentIndex, value);
        }

        public int SegmentCount => Route?.RouteSegmentSequence.Count ?? 0;

        public bool IsOnLoop
        {
            get => _isOnLoop;
            set
            {
                if (value == _isOnLoop) return;
                _isOnLoop = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalDistance));
            }
        }

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
            double loopDistance = 0;

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
                
                if (sequence.Type is SegmentSequenceType.Loop or SegmentSequenceType.LoopEnd or SegmentSequenceType.LoopStart)
                {
                    loopDistance += segment.Distance;
                }
            }

            TotalDistance = Math.Round(totalDistance / 1000, 1);
            _loopDistance = Math.Round(loopDistance / 1000, 1);
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
