// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private List<MarkerViewModel> _markers = new();
        private LoopMode _loopMode;
        private int? _numberOfLoops;

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
                this.RaisePropertyChanged();
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
                this.RaisePropertyChanged();
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
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ReadyToBuild));
            }
        }

        public bool ReadyToBuild => World != null && Sport != SportType.Unknown;

        public List<MarkerViewModel> Markers
        {
            get => _markers;
            private set
            {
                if (Equals(value, _markers)) return;
                _markers = value;
                this.RaisePropertyChanged();
            }
        }

        public LoopMode LoopMode
        {
            get => _loopMode;
            set
            {
                if (value == _loopMode) return;
                _loopMode = value;
                this.RaisePropertyChanged();
            }
        }

        public int? NumberOfLoops
        {
            get => _numberOfLoops;
            set
            {
                if (value == _numberOfLoops) return;
                _numberOfLoops = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsLoop
        {
            get
            {
                return Sequence.Any(s => s.IsLoop);
            }
        }

        public void StartOn(Segment segment)
        {
            if (_world == null)
            {
                throw new InvalidOperationException("Can't set a start segment because the world hasn't been selected yet");
            }

            if (_world.SpawnPoints == null)
            {
                throw new InvalidOperationException("Can't set a start segment because the world doesn't have any spawn points");
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
                new SegmentSequence(segmentId: segment.Id, turnToNextSegment: TurnDirection.None, nextSegmentId: null,
                    direction: segmentDirection, type: SegmentSequenceType.Regular),
                segment,
                _sequence.Count + 1)
            {
                Direction = segmentDirection
            });

            IsTainted = true;

            DetermineMarkersForRoute();

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
            var lastType = Last.Type;

            var newType = SegmentSequenceType.Regular;

            if (lastType == SegmentSequenceType.LoopEnd || lastType == SegmentSequenceType.LeadOut)
            {
                newType = SegmentSequenceType.LeadOut;
            }

            var segmentSequenceViewModel = new SegmentSequenceViewModel(
                new SegmentSequence(segmentId: ontoSegmentId, turnToNextSegment: TurnDirection.None,
                    nextSegmentId: null, direction: newSegmentDirection, type: newType),
                segment,
                _sequence.Count + 1)
            {
                Direction = newSegmentDirection,
            };

            _sequence.Add(segmentSequenceViewModel);

            IsTainted = true;

            DetermineMarkersForRoute();

            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));
        }

        public void Save()
        {
            IsTainted = false;
        }

        public PlannedRoute? AsPlannedRoute()
        {
            if (!Sequence.Any())
            {
                return null;
            }

            var zwiftRouteName = Sequence.Count() > 1
                ? GetZwiftRouteName(Sequence.First())
                : "Route starting at " + Sequence.First().SegmentId;

            var plannedRoute = new PlannedRoute
            {
                ZwiftRouteName = zwiftRouteName,
                Name = Name,
                World = World,
                Sport = Sport,
                LoopMode = LoopMode,
                NumberOfLoops = NumberOfLoops
            };

            if (string.IsNullOrEmpty(plannedRoute.Name))
            {
                plannedRoute.Name = $"RoadCaptain route starting on {plannedRoute.ZwiftRouteName}";
            }

            if (string.IsNullOrEmpty(plannedRoute.ZwiftRouteName))
            {
                throw new Exception(
                    $"Unable to determine Zwift route name for segment {Sequence.First().SegmentName} and direction {Sequence.First().Direction}");
            }

            plannedRoute
                .RouteSegmentSequence
                .AddRange(Sequence.Select(s => s.Model).ToList());

            plannedRoute.CalculateMetrics(_segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport));

            return plannedRoute;
        }

        private string GetZwiftRouteName(SegmentSequenceViewModel startingSegment)
        {
            if (_world == null)
            {
                throw new InvalidOperationException("Can't get route name because no world has been selected");
            }
            if (_world.SpawnPoints == null)
            {
                throw new InvalidOperationException("Can't get route name because the world doesn't have spawn points");
            }

            var spawnPoint = _world.SpawnPoints
                .SingleOrDefault(s =>
                    s.SegmentId == startingSegment.SegmentId && s.Direction == startingSegment.Direction);

            if (spawnPoint == null)
            {
                throw new InvalidOperationException($"No spawn point found for segment '{startingSegment.SegmentId}'");
            }

            return spawnPoint.ZwiftRouteName!;
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
            Markers.Clear();

            this.RaisePropertyChanged(nameof(Sequence));
            this.RaisePropertyChanged(nameof(TotalDistance));
            this.RaisePropertyChanged(nameof(TotalAscent));
            this.RaisePropertyChanged(nameof(TotalDescent));
            this.RaisePropertyChanged(nameof(Markers));

            return CommandResult.Success();
        }

        public bool IsSpawnPointSegment(string segmentId)
        {
            if (_world == null || _world.SpawnPoints == null)
            {
                return false;
            }

            return _world.SpawnPoints.Any(spawnPoint => spawnPoint.SegmentId == segmentId && (spawnPoint.Sport == SportType.Both || spawnPoint.Sport == Sport));
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(OutputFilePath))
            {
                throw new ArgumentException("Cannot load the route because no path was provided", nameof(OutputFilePath));
            }

            var plannedRoute = _routeStore.LoadFrom(OutputFilePath);
            
            if (plannedRoute.World == null)
            {
                throw new ArgumentException("Cannot load the route because the route has no world selected", nameof(World));
            }

            LoadFromPlannedRoute(plannedRoute);
        }

        public void LoadFromPlannedRoute(PlannedRoute plannedRoute, bool isTainted = false)
        {
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

            IsTainted = isTainted;

            // Don't use the properties because we don't
            // want PropertyChanged to fire just yet.
            _name = plannedRoute.Name;
            _world = plannedRoute.World;
            _sport = plannedRoute.Sport;

            DetermineMarkersForRoute();

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
                if (IsLoopTypeSegment(lastSegment))
                {
                    foreach (var seq in Sequence)
                    {
                        seq.Type = SegmentSequenceType.Regular;
                    }
                }

                IsTainted = _sequence.Any();

                DetermineMarkersForRoute();

                this.RaisePropertyChanged(nameof(Sequence));
                this.RaisePropertyChanged(nameof(TotalDistance));
                this.RaisePropertyChanged(nameof(TotalAscent));
                this.RaisePropertyChanged(nameof(TotalDescent));

                return lastSegment;
            }

            return null;
        }

        private static bool IsLoopTypeSegment(SegmentSequenceViewModel lastSegment)
        {
            switch (lastSegment.Type)
            {
                case SegmentSequenceType.LeadIn:
                case SegmentSequenceType.LeadOut:
                case SegmentSequenceType.Loop:
                case SegmentSequenceType.LoopStart:
                case SegmentSequenceType.LoopEnd:
                    return true;
                default:
                    return false;
            }
        }

        public (bool IsPossibleLoop, int? LoopStartIndex, int? LoopEndIndex) IsPossibleLoop()
        {
            if (Last == null || Sequence.Count() == 1 || Sequence.Any(s => s.IsLoop))
            {
                return (false, null, null);
            }

            // Depending on the direction on the segment the next nodes
            // that the last segment connects to are on NodeB or NodeA.
            var lastNode = Last.Direction == SegmentDirection.AtoB
                ? Last.Segment.NextSegmentsNodeB
                : Last.Segment.NextSegmentsNodeA;

            var nextSegmentIds = lastNode.Select(l => l.SegmentId).ToList();

            foreach (var nextSegment in nextSegmentIds)
            {
                var connectingSegmentsOnRoute = Sequence
                    .Where(seq => seq.SegmentId == nextSegment)
                    .MinBy(seq => seq.SequenceNumber);

                if (connectingSegmentsOnRoute != null)
                {
                    // Check if the next segment on the route after the match also connects to
                    // the last added segment. If that's the case apparently the route is 
                    // continuing in that direction and _that_ is the segment we'll continue
                    // on for the loop.
                    var segmentId = Sequence.ToList()[connectingSegmentsOnRoute.SequenceNumber].SegmentId;

                    if (segmentId == null)
                    {
                        throw new InvalidOperationException(
                            "Expected a segment id on the connecting segment but it was empty");
                    }

                    if(nextSegmentIds.Contains(segmentId))
                    {
                        connectingSegmentsOnRoute = Sequence.ToList()[connectingSegmentsOnRoute.SequenceNumber];
                    }

                    // For situations where the next segment is the one we came from
                    // we don't want to create a loop. This happens on mountain tops
                    // in Watopia where it makes no sense to create a loop because:
                    // - We don't allow you to go back to the segment you're currently on
                    // - You're already looping back the way you came up the mountain
                    if (segmentId == Last.SegmentId)
                    {
                        return (false, null, null);
                    }

                    return (true, connectingSegmentsOnRoute.SequenceNumber - 1, Last.SequenceNumber - 1);
                }
            }

            return (false, null, null);
        }

        public void MakeLoop(int startIndex, int endIndex, LoopMode mode, int? numberOfLoops)
        {
            var seqList = Sequence.ToList();

            for (var index = 0; index <= endIndex; index++)
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
            LoopMode = mode;
            NumberOfLoops = numberOfLoops;
            this.RaisePropertyChanged(nameof(IsLoop));
        }

        private void DetermineMarkersForRoute()
        {
            if (World == null || Sport == SportType.Unknown)
            {
                Markers = new List<MarkerViewModel>();
                return;
            }

            var plannedRoute = AsPlannedRoute();

            if (plannedRoute == null)
            {
                Markers = new List<MarkerViewModel>();
                return;
            }

            var markers = _segmentStore
                .LoadMarkers(World)
                .Where(m => m.Type == SegmentType.Climb || m.Type == SegmentType.Sprint)
                .ToList();

            Markers = PlannedRoute
                .CalculateClimbMarkers(markers, plannedRoute.TrackPoints.ToImmutableArray())
                .Select(marker => new MarkerViewModel(marker.Segment))
                .ToList();
        }
    }
}
