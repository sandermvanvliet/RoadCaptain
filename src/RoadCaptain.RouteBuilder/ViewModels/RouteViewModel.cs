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

            var route = new PlannedRoute
            {
                ZwiftRouteName = GetRouteName(Sequence.First())
            };

            if (string.IsNullOrEmpty(route.ZwiftRouteName))
            {
                throw new Exception($"Unable to determine Zwift route name for segment {Sequence.First().SegmentId} and direction {Sequence.First().Direction}");
            }

            route
                .RouteSegmentSequence
                .AddRange(Sequence.Select(s => s.Model).ToList());
            
            _routeStore.Store(route, OutputFilePath);
        }

        private string GetRouteName(SegmentSequenceViewModel startingSegment)
        {
            var spawnPoint = _spawnPoints
                .SingleOrDefault(s =>
                    s.SegmentId == startingSegment.SegmentId && s.SegmentDirection == startingSegment.Direction);

            return spawnPoint?.ZwiftRouteName;
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

        public bool IsSpawnPointSegment(string segmentId)
        {
            return _spawnPoints.Any(spanPoint => spanPoint.SegmentId == segmentId);
        }

        private readonly List<SpawnPoint> _spawnPoints = new()
        {
            new("watopia-bambino-fondo-001-after-after-after-after-after", "Beach Island Loop", SegmentDirection.BtoA),
            new("watopia-bambino-fondo-001-after-after-after-after-after", "Mountain Route", SegmentDirection.AtoB),
            new("watopia-bambino-fondo-004-before-before", "Mountain 8", SegmentDirection.AtoB),
            new("watopia-big-foot-hills-004-before", "Muir and the mountain", SegmentDirection.BtoA),
            new("watopia-big-foot-hills-004-before", "Big Foot Hills", SegmentDirection.AtoB),
            new("watopia-bambino-fondo-003-before-after", "Jungle Circuit", SegmentDirection.AtoB)
        };
    }

    internal class SpawnPoint
    {
        public string SegmentId { get; }
        public string ZwiftRouteName { get; }
        public SegmentDirection SegmentDirection { get; }

        public SpawnPoint(string segmentId, string zwiftRouteName, SegmentDirection segmentDirection)
        {
            SegmentId = segmentId;
            ZwiftRouteName = zwiftRouteName;
            SegmentDirection = segmentDirection;
        }
    }
}