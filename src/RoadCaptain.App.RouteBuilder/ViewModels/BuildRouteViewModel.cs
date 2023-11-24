// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class BuildRouteViewModel : ViewModelBase
    {
        private Segment? _selectedSegment;
        private Segment? _highlightedSegment;
        private Segment? _highlightedMarker;
        private List<Segment> _segments = new();
        private List<Segment> _markers = new();
        private TrackPoint? _riderPosition;
        private SimulationState _simulationState = SimulationState.NotStarted;
        private readonly IUserPreferences _userPreferences;
        private bool _showClimbs;
        private bool _showSprints;
        private bool _showElevationProfile;
        private readonly IWindowService _windowService;
        private Task? _simulationTask;

        public BuildRouteViewModel(RouteViewModel routeViewModel, IUserPreferences userPreferences,
            IWindowService windowService, ISegmentStore segmentStore, IStatusBarService statusBarService)
        {
            _userPreferences = userPreferences;
            _windowService = windowService;
            _showClimbs = _userPreferences.ShowClimbs;
            _showSprints = _userPreferences.ShowSprints;
            _showElevationProfile = _userPreferences.ShowElevationProfile;
            
            Route = routeViewModel;
            Route.PropertyChanged += (_, args) => HandleRoutePropertyChanged(segmentStore, args);
            
            RouteSegmentListViewModel = new RouteSegmentListViewModel(Route, windowService);
            
            SaveRouteCommand = new AsyncRelayCommand(
                    _ => SaveRoute(),
                    _ => !Route.IsReadOnly && Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.IsReadOnly)
                .OnSuccess(_ => statusBarService.Info("Route saved successfully"))
                .OnSuccessWithMessage(commandResult => statusBarService.Info($"Route saved successfully: {commandResult.Message}"))
                .OnFailure(commandResult => statusBarService.Error($"Failed to save route because: {commandResult.Message}"))
                .OnNotExecuted(_ => statusBarService.Info("Route hasn't changed dit not need to not saved"));

            ClearRouteCommand = new AsyncRelayCommand(
                    _ => ClearRoute(),
                    _ => Route.ReadyToBuild && Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.ReadyToBuild)
                .OnSuccess(_ => statusBarService.Info("Route cleared"))
                .OnFailure(commandResult => statusBarService.Error($"Failed to clear route because: {commandResult.Message}"));

            SelectSegmentCommand = new AsyncRelayCommand(
                    parameter => SelectSegment(parameter as Segment ??
                                       throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                    _ => true)
                .OnSuccess(_ => statusBarService.Info("Added segment"))
                .OnSuccessWithMessage(commandResult => statusBarService.Info($"Added segment {commandResult.Message}"))
                .OnFailure(commandResult => statusBarService.Warning(commandResult.Message));

            SimulateCommand = new RelayCommand(
                    _ => SimulateRoute(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence);

            RemoveLastSegmentCommand = new RelayCommand(
                    _ => RemoveLastSegment(),
                    _ => Route.IsTainted)
                .SubscribeTo(this, () => Route.Sequence)
                .OnSuccess(_ => statusBarService.Info("Removed segment"))
                .OnSuccessWithMessage(result => statusBarService.Info($"Removed segment {result.Message}"))
                .OnFailure(result => statusBarService.Warning(result.Message));

            ResetWorldCommand = new AsyncRelayCommand(
                    _ => ResetWorldAndSport(),
                    _ => Route.World != null)
                .SubscribeTo(this, () => Route.World);

            ToggleShowClimbsCommand = new AsyncRelayCommand(_ =>
                {
                    ShowClimbs = !ShowClimbs;
                    return Task.FromResult(CommandResult.Success());
                },
                _ => Route.World != null)
                .SubscribeTo(this, () => Route);

            ToggleShowSprintsCommand = new AsyncRelayCommand(_ =>
                {
                    ShowSprints = !ShowSprints;
                    return Task.FromResult(CommandResult.Success());
                },
                _ => Route.World != null)
                .SubscribeTo(this, () => Route);

            ToggleShowElevationCommand = new AsyncRelayCommand(_ =>
                {
                    ShowElevationProfile = !ShowElevationProfile;
                    return Task.FromResult(CommandResult.Success());
                },
                _ => Route.World != null)
                .SubscribeTo(this, () => Route);
        }
        
        public RouteViewModel Route { get; }
        public RouteSegmentListViewModel RouteSegmentListViewModel { get; }
        public ICommand SaveRouteCommand { get; }
        public ICommand ClearRouteCommand { get; }
        public ICommand SelectSegmentCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand RemoveLastSegmentCommand { get; }
        public ICommand ResetWorldCommand { get; }
        public ICommand ToggleShowClimbsCommand { get; }
        public ICommand ToggleShowSprintsCommand { get; }
        public ICommand ToggleShowElevationCommand { get; }

        public bool ShowClimbs
        {
            get => _showClimbs;
            private set
            {
                if (value == _showClimbs) return;
                _showClimbs = value;
                _userPreferences.ShowClimbs = _showClimbs;
                _userPreferences.Save();
                this.RaisePropertyChanged();
            }
        }

        public bool ShowSprints
        {
            get => _showSprints;
            private set
            {
                if (value == _showSprints) return;
                _showSprints = value;
                _userPreferences.ShowSprints = _showSprints;
                _userPreferences.Save();
                this.RaisePropertyChanged();
            }
        }

        public bool ShowElevationProfile
        {
            get => _showElevationProfile;
            private set
            {
                if (value == _showElevationProfile) return;
                _showElevationProfile = value;
                _userPreferences.ShowElevationProfile = _showElevationProfile;
                _userPreferences.Save();
                this.RaisePropertyChanged();
            }
        }

        public TrackPoint? RiderPosition
        {
            get => _riderPosition;
            set
            {
                _riderPosition = value;
                this.RaisePropertyChanged();
            }
        }

        public SimulationState SimulationState
        {
            get => _simulationState;
            set
            {
                _simulationState = value;
                this.RaisePropertyChanged();
            }
        }

        public List<Segment> Segments
        {
            get => _segments;
            set
            {
                if (value == _segments)
                {
                    return;
                }

                _segments = value;
                this.RaisePropertyChanged();
            }
        }

        public List<Segment> Markers
        {
            get => _markers;
            set
            {
                if (value == _markers)
                {
                    return;
                }

                _markers = value;

                this.RaisePropertyChanged();
            }
        }

        public Segment? HighlightedSegment
        {
            get => _highlightedSegment;
            set
            {
                if (value == _highlightedSegment)
                {
                    return;
                }

                _highlightedSegment = value;
                this.RaisePropertyChanged();
            }
        }

        public Segment? SelectedSegment
        {
            get => _selectedSegment;
            private set
            {
                if (value == _selectedSegment) return;
                _selectedSegment = value;
                this.RaisePropertyChanged();
            }
        }

        public Segment? HighlightedMarker
        {
            get => _highlightedMarker;
            private set
            {
                if (value == _highlightedMarker) return;
                _highlightedMarker = value;
                this.RaisePropertyChanged();
            }
        }

        private async Task<CommandResult> ClearRoute()
        {
            if (Route.IsTainted)
            {
                var result = await _windowService.ShowClearRouteDialog();

                if (result == MessageBoxResult.No)
                {
                    return CommandResult.Aborted();
                }
            }

            var commandResult = Route.Clear();

            SelectedSegment = null;
            HighlightedSegment = null;
            HighlightedMarker = null;

            SimulationState = SimulationState.NotStarted;

            this.RaisePropertyChanged(nameof(Route));

            return commandResult;
        }

        private CommandResult SimulateRoute()
        {
            if (_simulationTask != null && SimulationState == SimulationState.Running)
            {
                SimulationState = SimulationState.Completed;
                RiderPosition = null;
                return CommandResult.Success();
            }

            _simulationTask = Task.Factory.StartNew(() =>
            {
                SimulationState = SimulationState.Running;

                var routePoints = new List<TrackPoint>();

                foreach (var seq in Route.Sequence)
                {
                    var points = _segments.Single(s => s.Id == seq.SegmentId).Points;

                    if (seq.Direction == SegmentDirection.BtoA)
                    {
                        // Don't call Reverse() because that does an
                        // in-place reverse and given that we're 
                        // _referencing_ the list of points of the
                        // segment that means that the actual segment
                        // is modified. Reverse() does not return a
                        // new IEnumerable<T>
                        points = points.OrderByDescending(p => p.Index).ToList();
                    }

                    routePoints.AddRange(points);
                }

                var simulationIndex = 0;

                while (SimulationState == SimulationState.Running && simulationIndex < routePoints.Count)
                {
                    RiderPosition = routePoints[simulationIndex++];
                    Thread.Sleep(40);
                }

                SimulationState = SimulationState.Completed;
                RiderPosition = null;
            });

            return CommandResult.Success();
        }

        private async Task<CommandResult> SaveRoute()
        {
            try
            {
                await _windowService.ShowSaveRouteDialog(_userPreferences.LastUsedFolder, Route);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure("Failed to save route: " + e.Message);
            }
        }

        public void Reset()
        {
            SelectedSegment = null;
            HighlightedSegment = null;
            HighlightedMarker = null;

            SimulationState = SimulationState.NotStarted;

            Segments = new List<Segment>();

            Markers = new();
        }

        protected async Task<CommandResult> SelectSegment(Segment newSelectedSegment)
        {
            var segmentId = newSelectedSegment.Id;

            // 1. Figure out if this is the first segment on the route, if so add it to the route and set the selection to the new segment
            if (!Route.Sequence.Any())
            {
                if (!Route.IsSpawnPointSegment(segmentId))
                {
                    return CommandResult.Failure(
                        $"{newSelectedSegment.Name} is not a spawn point, we can't start here unfortunately");
                }

                Route.StartOn(newSelectedSegment);

                SelectedSegment = newSelectedSegment;

                return CommandResult.SuccessWithMessage(newSelectedSegment.Name);
            }

            // Prevent selecting the same segment again
#pragma warning disable CS8602 // Because the check on Route.Sequence.Any() already ensures that Route.Last can't be null
            if (Route.Last.SegmentId == segmentId)
#pragma warning restore CS8602
            {
                return CommandResult.Aborted();
            }

            if (!string.IsNullOrEmpty(newSelectedSegment.NoSelectReason))
            {
                return CommandResult.Failure(newSelectedSegment.NoSelectReason);
            }

            // 2. Figure out if the newly selected segment is reachable from the last segment
            var lastSegment = Segments.Single(s => s.Id == Route.Last.SegmentId);

            var fromA = lastSegment.NextSegmentsNodeA.SingleOrDefault(t => t.SegmentId == newSelectedSegment.Id);
            var fromB = lastSegment.NextSegmentsNodeB.SingleOrDefault(t => t.SegmentId == newSelectedSegment.Id);

            if (Route.Last.Direction == SegmentDirection.AtoB)
            {
                if (fromB != null)
                {
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment, Route.Last.Direction);

                    Route.NextStep(fromB.Direction, fromB.SegmentId, newSelectedSegment, SegmentDirection.AtoB,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    await CheckForPossibleLoop();

                    return CommandResult.SuccessWithMessage(newSelectedSegment.Name);
                }
            }
            else if (Route.Last.Direction == SegmentDirection.BtoA)
            {
                if (fromA != null)
                {
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment, Route.Last.Direction);

                    Route.NextStep(fromA.Direction, fromA.SegmentId, newSelectedSegment, SegmentDirection.BtoA,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    await CheckForPossibleLoop();

                    return CommandResult.SuccessWithMessage(newSelectedSegment.Name);
                }
            }
            else if (Route.Last.Direction == SegmentDirection.Unknown)
            {
                if (fromA != null)
                {
                    if (!IsValidSpawnPointProgression(out var commandResult, SegmentDirection.BtoA))
                    {
                        return commandResult;
                    }

                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment, Route.Last.Direction);

                    Route.NextStep(fromA.Direction, fromA.SegmentId, newSelectedSegment, SegmentDirection.BtoA,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    await CheckForPossibleLoop();

                    return CommandResult.SuccessWithMessage(newSelectedSegment.Name);
                }

                if (fromB != null)
                {
                    if (!IsValidSpawnPointProgression(out var commandResult, SegmentDirection.AtoB))
                    {
                        return commandResult;
                    }

                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment, Route.Last.Direction);

                    Route.NextStep(fromB.Direction, fromB.SegmentId, newSelectedSegment, SegmentDirection.AtoB,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    await CheckForPossibleLoop();

                    return CommandResult.SuccessWithMessage(newSelectedSegment.Name);
                }
            }

            if (Route.Sequence.Count() == 1)
            {
                return CommandResult.Failure("Spawn point does not support the direction of the selected segment");
            }

            return CommandResult.Failure(
                "Did not find a connection between the last segment and the selected segment");
        }

        private async Task CheckForPossibleLoop()
        {
            var (isLoop, startIndex, endIndex) = Route.IsPossibleLoop();

            if (isLoop && startIndex.HasValue && endIndex.HasValue)
            {
                var shouldCreateLoop = await _windowService.ShowRouteLoopDialog();

                if (shouldCreateLoop.Mode is LoopMode.Infinite or LoopMode.Constrained)
                {
                    Route.MakeLoop(startIndex.Value, endIndex.Value, shouldCreateLoop.Mode, shouldCreateLoop.NumberOfLoops);
                    this.RaisePropertyChanged(nameof(Route));
                }
            }
        }

        private bool IsValidSpawnPointProgression(out CommandResult commandResult, SegmentDirection segmentDirection)
        {
            if (Route.World != null && Route.Sequence.Count() == 1 && Route.World.SpawnPoints != null)
            {
                var startSegmentSequence = Route.Sequence.First();
                if (!Route.World.SpawnPoints.Any(sp =>
                        sp.SegmentId == startSegmentSequence.SegmentId &&
                        sp.Direction == segmentDirection))
                {
                    {
                        commandResult =
                            CommandResult.Failure("Spawn point does not support the direction of the selected segment");
                        return false;
                    }
                }
            }

            commandResult = new CommandResult { Result = Result.NotExecuted };

            return true;
        }

        private static SegmentDirection GetDirectionOnNewSegment(Segment newSelectedSegment, Segment lastSegment,
            SegmentDirection segmentDirection)
        {
            var newSegmentDirection = SegmentDirection.Unknown;

            var linkOnNodeA = newSelectedSegment.NextSegmentsNodeA.Any(s => s.SegmentId == lastSegment.Id);
            var linkOnNodeB = newSelectedSegment.NextSegmentsNodeB.Any(s => s.SegmentId == lastSegment.Id);

            if (linkOnNodeA && linkOnNodeB)
            {
                if (segmentDirection == SegmentDirection.AtoB)
                {
                    if (lastSegment.B.IsCloseTo(newSelectedSegment.A))
                    {
                        return SegmentDirection.AtoB;
                    }

                    if (lastSegment.B.IsCloseTo(newSelectedSegment.B))
                    {
                        return SegmentDirection.BtoA;
                    }
                }
                else if (segmentDirection == SegmentDirection.BtoA)
                {
                    if (lastSegment.A.IsCloseTo(newSelectedSegment.A))
                    {
                        return SegmentDirection.AtoB;
                    }

                    if (lastSegment.A.IsCloseTo(newSelectedSegment.B))
                    {
                        return SegmentDirection.BtoA;
                    }
                }
            }
            else if (linkOnNodeA)
            {
                newSegmentDirection = SegmentDirection.AtoB;
            }
            else if (linkOnNodeB)
            {
                newSegmentDirection = SegmentDirection.BtoA;
            }

            return newSegmentDirection;
        }

        public void HighlightSegment(string segmentId)
        {
            if (string.IsNullOrEmpty(segmentId))
            {
                HighlightedSegment = null;
            }
            else
            {
                HighlightedSegment = Segments.SingleOrDefault(s => s.Id == segmentId);
            }
        }

        public void HighlightMarker(string markerId)
        {
            if (string.IsNullOrEmpty(markerId))
            {
                HighlightedSegment = null;
            }
            else
            {
                HighlightedMarker = Markers.SingleOrDefault(s => s.Id == markerId);
            }
        }

        public void ClearSegmentHighlight()
        {
            HighlightedSegment = null;
        }

        public void ClearMarkerHighlight()
        {
            HighlightedMarker = null;
        }

        private void HandleRoutePropertyChanged(ISegmentStore segmentStore, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Route.World):
                {
                    if (Route.World == null)
                    {
                        Segments = new List<Segment>();
                    }

                    TryLoadSegmentsForWorldAndSport(segmentStore);
                    break;
                }
                case nameof(Route.Sport):
                {
                    if (Route.Sport == SportType.Unknown)
                    {
                        Segments = new List<Segment>();
                    }

                    TryLoadSegmentsForWorldAndSport(segmentStore);
                    break;
                }
                case nameof(Route.ReadyToBuild):
                case nameof(Route.Sequence):
                    this.RaisePropertyChanged(nameof(Route));
                    break;
            }
        }

        private void TryLoadSegmentsForWorldAndSport(ISegmentStore segmentStore)
        {
            if (Route is { ReadyToBuild: true, World: not null } && Route.Sport != SportType.Unknown)
            {
                Segments = segmentStore.LoadSegments(Route.World, Route.Sport);
                Markers = segmentStore.LoadMarkers(Route.World);
            }
        }

        private CommandResult RemoveLastSegment()
        {
            if (!Route.Sequence.Any())
            {
                return CommandResult.Failure("Can't remove segment because the route does not have any segments");
            }

            var lastSegment = Route.RemoveLast();

            SelectedSegment = null;

            if (lastSegment != null)
            {
                return CommandResult.SuccessWithMessage(lastSegment.SegmentName);
            }

            return CommandResult.Success();
        }

        private async Task<CommandResult> ResetWorldAndSport()
        {
            if (Route.IsTainted)
            {
                var result = await _windowService.ShowShouldSaveRouteDialog();
            
                if (result == MessageBoxResult.Cancel)
                {
                    return CommandResult.Aborted();
                }
            
                if (result == MessageBoxResult.Yes)
                {
                    SaveRouteCommand.Execute(null);
                }
            }
            
            Route.Reset();
            Reset();

            return CommandResult.Success();
        }
    }
}
