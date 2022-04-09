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
        private readonly IRouteStore _routeStore;
        private readonly List<Segment> _segments;
        private readonly ObservableCollection<SegmentSequenceViewModel> _sequence = new();

        private readonly List<SpawnPoint> _spawnPoints = new()
        {
            new SpawnPoint("watopia-bambino-fondo-001-after-after-after-after-after", "Beach Island Loop",
                SegmentDirection.BtoA),
            new SpawnPoint("watopia-bambino-fondo-001-after-after-after-after-after", "Mountain Route",
                SegmentDirection.AtoB),
            new SpawnPoint("watopia-bambino-fondo-004-before-before", "The Mega Pretzel", SegmentDirection.AtoB),
            new SpawnPoint("watopia-big-foot-hills-004-before", "Muir and the mountain", SegmentDirection.BtoA),
            new SpawnPoint("watopia-big-foot-hills-004-before", "Big Foot Hills", SegmentDirection.AtoB),
            new SpawnPoint("watopia-bambino-fondo-003-before-after", "Jungle Circuit", SegmentDirection.AtoB)
        };

        private string _name;

        public RouteViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            _routeStore = routeStore;
            _segments = segmentStore.LoadSegments();
        }

        public IEnumerable<SegmentSequenceViewModel> Sequence => _sequence;

        public double TotalDistance => Math.Round(Sequence.Sum(s => s.Distance), 1);
        public double TotalAscent => Math.Round(Sequence.Sum(s => s.Ascent), 1);
        public double TotalDescent => Math.Round(Sequence.Sum(s => s.Descent), 1);

        public SegmentSequenceViewModel Last => Sequence.LastOrDefault();
        public string OutputFilePath { get; set; }
        public bool IsTainted { get; private set; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void StartOn(Segment segment)
        {
            _sequence.Add(new SegmentSequenceViewModel(
                new SegmentSequence
                {
                    SegmentId = segment.Id,
                    TurnToNextSegment = TurnDirection.None,
                    NextSegmentId = null
                },
                segment,
                _sequence.Count + 1));

            IsTainted = true;

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
                _sequence.Count + 1)
            {
                Direction = newSegmentDirection
            };

            _sequence.Add(segmentSequenceViewModel);

            IsTainted = true;

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(OutputFilePath))
            {
                throw new ArgumentException("Output file path is empty");
            }

            var route = new PlannedRoute
            {
                ZwiftRouteName = GetRouteName(Sequence.First()),
                Name = Name
            };

            if (string.IsNullOrEmpty(route.Name))
            {
                route.Name = $"RoadCaptain route starting on {route.ZwiftRouteName}";
            }

            if (string.IsNullOrEmpty(route.ZwiftRouteName))
            {
                throw new Exception(
                    $"Unable to determine Zwift route name for segment {Sequence.First().SegmentId} and direction {Sequence.First().Direction}");
            }

            route
                .RouteSegmentSequence
                .AddRange(Sequence.Select(s => s.Model).ToList());

            _routeStore.Store(route, OutputFilePath);

            IsTainted = false;
        }

        private string GetRouteName(SegmentSequenceViewModel startingSegment)
        {
            var spawnPoint = _spawnPoints
                .SingleOrDefault(s =>
                    s.SegmentId == startingSegment.SegmentId && s.SegmentDirection == startingSegment.Direction);

            return spawnPoint?.ZwiftRouteName;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CommandResult Reset()
        {
            _sequence.Clear();
            OutputFilePath = null;
            IsTainted = false;

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

        public void Load()
        {
            var plannedRoute = _routeStore.LoadFrom(OutputFilePath);

            _sequence.Clear();

            foreach (var seq in plannedRoute.RouteSegmentSequence)
            {
                _sequence.Add(
                    new SegmentSequenceViewModel(
                        seq,
                        GetSegmentById(seq.SegmentId),
                        _sequence.Count + 1)
                    {
                        Direction = seq.Direction
                    });
            }

            Name = plannedRoute.Name;
            IsTainted = false;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        private Segment GetSegmentById(string segmentId)
        {
            return _segments.SingleOrDefault(s => s.Id == segmentId);
        }
    }
}