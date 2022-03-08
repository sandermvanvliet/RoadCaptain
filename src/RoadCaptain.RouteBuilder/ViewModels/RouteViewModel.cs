using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.Ports;
using RoadCaptain.RouteBuilder.Annotations;
using RoadCaptain.RouteBuilder.Commands;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class RouteViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<SegmentSequenceViewModel> _sequence = new();
        private readonly IRouteStore _routeStore;

        public RouteViewModel(IRouteStore routeStore)
        {
            _routeStore = routeStore;
        }

        public IEnumerable<SegmentSequenceViewModel> Sequence => _sequence;

        public double TotalDistance => Math.Round(Sequence.Sum(s => s.Distance) ,1);
        public double TotalAscent => Math.Round(Sequence.Sum(s => s.Ascent), 1);
        public double TotalDescent => Math.Round(Sequence.Sum(s => s.Descent), 1);

        public SegmentSequenceViewModel Last => Sequence.LastOrDefault();
        public string OutputFilePath { get; set; }

        public void StartOn(Segment segment)
        {
            _sequence.Add(new SegmentSequenceViewModel(new SegmentSequence
                {
                    SegmentId = segment.Id,
                    TurnToNextSegment = TurnDirection.None,
                    NextSegmentId = null
                },
                segment,
                _sequence.Count + 1));

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public void NextStep(TurnDirection direction,
            string ontoSegmentId,
            Segment segment,
            SegmentDirection segmentDirection, 
            SegmentDirection newSegmentDirection)
        {
            Last.SetTurn(direction, ontoSegmentId, segmentDirection);

            var segmentSequenceViewModel = new SegmentSequenceViewModel(
                new SegmentSequence
                {
                    SegmentId = ontoSegmentId,
                    TurnToNextSegment = TurnDirection.None,
                    NextSegmentId = null,
                    Direction = newSegmentDirection
                },
                segment,
                _sequence.Count + 1);

            segmentSequenceViewModel.Direction = newSegmentDirection;

            _sequence.Add(segmentSequenceViewModel);

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public void Save()
        {
            if(string.IsNullOrEmpty(OutputFilePath))
            {
                throw new ArgumentException("Output file path is empty");
            }

            var route = new PlannedRoute();

            route
                .RouteSegmentSequence
                .AddRange(Sequence.Select(s => s.Model).ToList());
            
            _routeStore.Store(route, OutputFilePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CommandResult Reset()
        {
            _sequence.Clear();

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));

            return CommandResult.Success();
        }
    }
}