using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.Ports;
using RoadCaptain.UserInterface.Shared.Commands;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class RouteViewModel : INotifyPropertyChanged
    {
        private readonly IRouteStore _routeStore;
        private readonly ISegmentStore _segmentStore;
        private readonly ObservableCollection<SegmentSequenceViewModel> _sequence = new();

        private string _name;
        private World _world;
        private SportType _sport;

        public RouteViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            _routeStore = routeStore;
            _segmentStore = segmentStore;
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

        public World World
        {
            get => _world;
            set
            {
                if (value == _world)
                {
                    return;
                }

                _world = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReadyToBuild));
            }
        }

        public SportType Sport
        {
            get => _sport;
            set
            {
                if (value == _sport)
                {
                    return;
                }

                _sport = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReadyToBuild));
            }
        }

        public bool ReadyToBuild => World != null && Sport != SportType.Unknown;

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
                ZwiftRouteName = GetZwiftRouteName(Sequence.First()),
                Name = Name,
                World = World,
                Sport = Sport
            };

            if (string.IsNullOrEmpty(route.Name))
            {
                route.Name = $"RoadCaptain route starting on {route.ZwiftRouteName}";
            }

            if (string.IsNullOrEmpty(route.ZwiftRouteName))
            {
                throw new Exception(
                    $"Unable to determine Zwift route name for segment {Sequence.First().SegmentName} and direction {Sequence.First().Direction}");
            }

            route
                .RouteSegmentSequence
                .AddRange(Sequence.Select(s => s.Model).ToList());

            _routeStore.Store(route, OutputFilePath);

            IsTainted = false;
        }

        private string GetZwiftRouteName(SegmentSequenceViewModel startingSegment)
        {
            var spawnPoint = _world.SpawnPoints
                .SingleOrDefault(s =>
                    s.SegmentId == startingSegment.SegmentId && s.Direction == startingSegment.Direction);

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
            World = null;
            Sport = SportType.Unknown;

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));

            return CommandResult.Success();
        }

        public bool IsSpawnPointSegment(string segmentId)
        {
            if (_world == null)
            {
                return false;
            }
            return _world.SpawnPoints.Any(spanPoint => spanPoint.SegmentId == segmentId);
        }

        public void Load()
        {
            var plannedRoute = _routeStore.LoadFrom(OutputFilePath);
            var segments = _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport);

            _sequence.Clear();

            foreach (var seq in plannedRoute.RouteSegmentSequence)
            {
                _sequence.Add(
                    new SegmentSequenceViewModel(
                        seq,
                        segments.Single(s => s.Id == seq.SegmentId),
                        _sequence.Count + 1)
                    {
                        Direction = seq.Direction,
                    });
            }

            Name = plannedRoute.Name;
            IsTainted = false;
            World = plannedRoute.World;
            Sport = plannedRoute.Sport;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public SegmentSequenceViewModel RemoveLast()
        {
            var lastSegment = Last;

            if (lastSegment == _sequence.First())
            {
                Reset();
            }
            else
            {
                _sequence.Remove(lastSegment);

                Last.ResetTurn();
            }
            

            IsTainted = _sequence.Any();

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));

            return lastSegment;
        }
    }
}