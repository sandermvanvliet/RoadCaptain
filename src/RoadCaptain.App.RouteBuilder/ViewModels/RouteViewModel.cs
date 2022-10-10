// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using CommandResult = RoadCaptain.App.Shared.Commands.CommandResult;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class RouteViewModel : ViewModelBase
    {
        private readonly IRouteStore _routeStore;
        private readonly ISegmentStore _segmentStore;
        private readonly ObservableCollection<SegmentSequenceViewModel> _sequence = new();

        private string? _name;
        private World? _world;
        private SportType _sport = SportType.Unknown;

        public RouteViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            _routeStore = routeStore;
            _segmentStore = segmentStore;
        }

        public IEnumerable<SegmentSequenceViewModel> Sequence => _sequence;

        public double TotalDistance => Math.Round(Sequence.Sum(s => s.Distance), 1);
        public double TotalAscent => Math.Round(Sequence.Sum(s => s.Ascent), 1);
        public double TotalDescent => Math.Round(Sequence.Sum(s => s.Descent), 1);

        public SegmentSequenceViewModel? Last => Sequence.LastOrDefault();
        public string? OutputFilePath { get; set; }
        public bool IsTainted { get; private set; }

        public string Name
        {
            get => _name ?? string.Empty;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }

        public World? World
        {
            get => _world;
            set
            {
                if (value == _world)
                {
                    return;
                }

                _world = value;
                this.RaisePropertyChanged(nameof(World));
                this.RaisePropertyChanged(nameof(ReadyToBuild));
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
                this.RaisePropertyChanged(nameof(Sport));
                this.RaisePropertyChanged(nameof(ReadyToBuild));
            }
        }

        public bool ReadyToBuild => World != null && Sport != SportType.Unknown;

        public void StartOn(Segment segment)
        {
            if (_world == null)
            {
                throw new InvalidOperationException("Can't set a start segment because the world hasn't been selected yet");
            }

            var segmentDirection = SegmentDirection.Unknown;

            var routesStartingOnSegment = _world.SpawnPoints.Where(s => s.SegmentId == segment.Id).Select(s => s.Direction).ToList();

            if (routesStartingOnSegment.Count == 1)
            {
                segmentDirection = routesStartingOnSegment[0];
            }
            else if (routesStartingOnSegment.Distinct().Count() == 1)
            {
                segmentDirection = routesStartingOnSegment[0];
            }

            _sequence.Add(new SegmentSequenceViewModel(
                new SegmentSequence
                {
                    SegmentId = segment.Id,
                    TurnToNextSegment = TurnDirection.None,
                    NextSegmentId = null,
                    Direction = segmentDirection,
                    Type = SegmentSequenceType.Regular
                },
                segment,
                _sequence.Count + 1)
            {
                Direction = segmentDirection
            });

            IsTainted = true;

            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));
        }

        public void NextStep(TurnDirection direction,
            string ontoSegmentId,
            Segment segment,
            SegmentDirection segmentDirection,
            SegmentDirection newSegmentDirection)
        {
            if (Last == null)
            {
                throw new InvalidOperationException("Can't set turn on last segment because there is no last segment");
            }

            Last.SetTurn(direction, ontoSegmentId, segmentDirection);

            var segmentSequenceViewModel = new SegmentSequenceViewModel(
                new SegmentSequence
                {
                    SegmentId = ontoSegmentId,
                    TurnToNextSegment = TurnDirection.None,
                    NextSegmentId = null,
                    Direction = newSegmentDirection,
                    Type = SegmentSequenceType.Regular
                },
                segment,
                _sequence.Count + 1)
            {
                Direction = newSegmentDirection,
            };

            _sequence.Add(segmentSequenceViewModel);

            IsTainted = true;

            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));
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
            if (_world == null)
            {
                throw new InvalidOperationException("Can't get route name because no world has been selected");
            }

            var spawnPoint = _world.SpawnPoints
                .SingleOrDefault(s =>
                    s.SegmentId == startingSegment.SegmentId && s.Direction == startingSegment.Direction);

            if (spawnPoint == null)
            {
                throw new InvalidOperationException($"No spawn point found for segment '{startingSegment.SegmentId}'");
            }

            return spawnPoint.ZwiftRouteName;
        }

        public CommandResult Reset()
        {
            World = null;
            Sport = SportType.Unknown;
            
            this.RaisePropertyChanged(nameof(ReadyToBuild));

            return Clear();
        }

        public CommandResult Clear()
        {
            _sequence.Clear();
            OutputFilePath = null;
            IsTainted = false;
            Name = string.Empty;

            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));

            return CommandResult.Success();
        }

        public bool IsSpawnPointSegment(string segmentId)
        {
            if (_world == null)
            {
                return false;
            }

            return _world.SpawnPoints.Any(spawnPoint => spawnPoint.SegmentId == segmentId && (spawnPoint.Sport == SportType.Both || spawnPoint.Sport == Sport));
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
                        Direction = seq.Direction
                    });
            }

            IsTainted = false;

            // Don't use the properties because we don't
            // want PropertyChanged to fire just yet.
            _name = plannedRoute.Name;
            _world = plannedRoute.World;
            _sport = plannedRoute.Sport;

            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(World));
            this.RaisePropertyChanged(nameof(Sport));
            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));
            this.RaisePropertyChanged(nameof(ReadyToBuild));
        }

        public SegmentSequenceViewModel? RemoveLast()
        {
            if (Last != null)
            {
                var lastSegment = Last;

                if (lastSegment == _sequence.First())
                {
                    _sequence.Clear();
                }
                else
                {
                    _sequence.Remove(lastSegment);

                    Last.ResetTurn();
                }

                // If the last segment of a loop is removed then reset
                // the segment sequence type to regular because the
                // loop has been broken.
                if (lastSegment.Type == SegmentSequenceType.Loop || lastSegment.Type == SegmentSequenceType.LeadIn)
                {
                    foreach (var seq in Sequence)
                    {
                        seq.Model.Type = SegmentSequenceType.Regular;
                    }
                }

                IsTainted = _sequence.Any();

                this.RaisePropertyChanged(nameof(Sequence));
                this.RaisePropertyChanged(nameof(TotalDistance));
                this.RaisePropertyChanged(nameof(TotalAscent));
                this.RaisePropertyChanged(nameof(TotalDescent));

                return lastSegment;
            }

            return null;
        }

        public (bool,int?, int?) IsPossibleLoop()
        {
            if (Last == null || Sequence.Count() == 1)
            {
                return (false, null, null);
            }

            var startNodes = Sequence
                .Select((seq, index) =>
                    new
                    {
                        Index = index,
                        SegmentId = seq.SegmentId,
                        Direction = seq.Direction,
                        StartNode = seq.Direction == SegmentDirection.AtoB
                            ? seq.Segment.NextSegmentsNodeA
                            : seq.Segment.NextSegmentsNodeB,
                        Seq = seq
                    })
                .ToList();
            
            foreach (var seq in startNodes)
            {
                if (seq.StartNode.Any(n => n.SegmentId == Last.SegmentId))
                {
                    return (true, seq.Index, Sequence.Count() - 1);
                }
            }

            return (false, null, null);
        }

        public void MakeLoop(int startIndex, int endIndex)
        {
            var seqList = Sequence.ToList();

            for(var index = 0; index <= endIndex; index++)
            {
                var type = SegmentSequenceType.Loop;
                
                if (index < startIndex)
                {
                    type = SegmentSequenceType.LeadIn;
                }
                else if (index == startIndex)
                {
                    type = SegmentSequenceType.LoopStart;
                }
                else if (index == endIndex)
                {
                    type = SegmentSequenceType.LoopEnd;
                }

                seqList[index].Type = type;
            }

            seqList[endIndex].Model.NextSegmentId = seqList[startIndex].SegmentId;
        }
    }
}
