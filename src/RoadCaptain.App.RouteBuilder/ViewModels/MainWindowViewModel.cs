// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.UseCases;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs;
using CommandResult = RoadCaptain.App.Shared.Commands.CommandResult;
using RelayCommand = RoadCaptain.App.Shared.Commands.RelayCommand;
using Result = RoadCaptain.App.Shared.Commands.Result;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Segment? _selectedSegment;
        private List<Segment> _segments;
        private Task? _simulationTask;
        private TrackPoint? _riderPosition;
        private SimulationState _simulationState = SimulationState.NotStarted;
        private string? _version;
        private string? _changelogUri;
        private bool _haveCheckedVersion;
        private readonly IVersionChecker _versionChecker;
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;
        private List<Segment> _markers;
        private bool _showClimbs;
        private bool _showSprints;
        private Segment? _highlightedSegment;
        private bool _haveCheckedLastOpenedVersion;
        private bool _showElevationProfile;
        private Segment? _highlightedMarker;
        private readonly IApplicationFeatures _applicationFeatures;
        private readonly ConvertZwiftMapRouteUseCase _convertUseCase;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker,
            IWindowService windowService, IWorldStore worldStore, IUserPreferences userPreferences, IApplicationFeatures applicationFeatures)
        {
            _segments = new List<Segment>();
            _versionChecker = versionChecker;
            _windowService = windowService;
            _userPreferences = userPreferences;
            _applicationFeatures = applicationFeatures;
            _convertUseCase = new ConvertZwiftMapRouteUseCase(worldStore, segmentStore);
            _showClimbs = _userPreferences.ShowClimbs;
            _showSprints = _userPreferences.ShowSprints;
            _showElevationProfile = _userPreferences.ShowElevationProfile;

            Model = new MainWindowModel();
            _markers = new List<Segment>();
            var segmentStore1 = segmentStore;
            Route = new RouteViewModel(routeStore, segmentStore1);
            LandingPageViewModel = new LandingPageViewModel(worldStore, userPreferences, windowService);
            RouteSegmentListViewModel = new RouteSegmentListViewModel(Route, windowService);
            
            Route.PropertyChanged += (_, args) => HandleRoutePropertyChanged(segmentStore1, args);

            SelectDefaultSportFromPreferences();

            SaveRouteCommand = new AsyncRelayCommand(
                    _ => SaveRoute(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .OnSuccess(_ => Model.StatusBarInfo("Route saved successfully"))
                .OnSuccessWithMessage(_ => Model.StatusBarInfo("Route saved successfully: {0}", _.Message))
                .OnFailure(_ => Model.StatusBarError("Failed to save route because: {0}", _.Message))
                .OnNotExecuted(_ => Model.StatusBarInfo("Route hasn't changed dit not need to not saved"));

            OpenRouteCommand = new AsyncRelayCommand(
                    _ => OpenRoute(),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Route loaded successfully"))
                .OnSuccessWithMessage(_ => Model.StatusBarInfo(_.Message))
                .OnFailure(_ => Model.StatusBarError("Failed to load route because: {0}", _.Message));

            ClearRouteCommand = new AsyncRelayCommand(
                    _ => ClearRoute(),
                    _ => Route.ReadyToBuild && Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.ReadyToBuild)
                .OnSuccess(_ => Model.StatusBarInfo("Route cleared"))
                .OnFailure(_ => Model.StatusBarError("Failed to clear route because: {0}", _.Message));

            SelectSegmentCommand = new AsyncRelayCommand(
                    _ => SelectSegment(_ as Segment ??
                                       throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Added segment"))
                .OnSuccessWithMessage(_ => Model.StatusBarInfo("Added segment {0}", _.Message))
                .OnFailure(_ => Model.StatusBarWarning(_.Message));

            SimulateCommand = new RelayCommand(
                    _ => SimulateRoute(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence);

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => !string.IsNullOrEmpty(_ as string));

            ResetWorldCommand = new AsyncRelayCommand(
                    _ => ResetWorldAndSport(),
                    _ => Route.World != null)
                .SubscribeTo(this, () => Route.World);

            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";
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
                    await SaveRoute();
                }
            }

            Route.Reset();

            SelectedSegment = null;
            HighlightedSegment = null;
            HighlightedMarker = null;

            SimulationState = SimulationState.NotStarted;

            Segments = new List<Segment>();

            Markers = new();

            var selectedSport = LandingPageViewModel.Sports.SingleOrDefault(s => s.IsSelected);
            if (selectedSport != null)
            {
                selectedSport.IsSelected = false;

                SelectDefaultSportFromPreferences();
            }

            var selectedWorld = LandingPageViewModel.Worlds.SingleOrDefault(s => s.IsSelected);
            if (selectedWorld != null)
            {
                selectedWorld.IsSelected = false;
            }

            this.RaisePropertyChanged(nameof(Route));

            return CommandResult.Success();
        }

        private void SelectDefaultSportFromPreferences()
        {
            if (LandingPageViewModel.HasDefaultSport)
            {
                var sport = LandingPageViewModel
                    .Sports
                    .SingleOrDefault(s =>
                        (_userPreferences.DefaultSport ?? "").Equals(s.Sport.ToString(),
                            StringComparison.InvariantCultureIgnoreCase));

                if (sport != null)
                {
                    sport.IsSelected = true;
                    sport.IsDefault = true;
                    Route.Sport = sport.Sport;
                }
            }
        }

        private void HandleRoutePropertyChanged(ISegmentStore segmentStore, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Route.World))
            {
                if (Route.World == null)
                {
                    Segments = new List<Segment>();
                }

                TryLoadSegmentsForWorldAndSport(segmentStore);
            }

            if (args.PropertyName == nameof(Route.Sport))
            {
                if (Route.Sport == SportType.Unknown)
                {
                    Segments = new List<Segment>();
                }

                TryLoadSegmentsForWorldAndSport(segmentStore);
            }

            if (args.PropertyName == nameof(Route.Sequence))
            {
                this.RaisePropertyChanged(nameof(Route));
            }
        }

        private void TryLoadSegmentsForWorldAndSport(ISegmentStore segmentStore)
        {
            if (Route.World != null && Route.Sport != SportType.Unknown)
            {
                Segments = segmentStore.LoadSegments(Route.World, Route.Sport);
                Markers = segmentStore.LoadMarkers(Route.World);
            }
        }

        public MainWindowModel Model { get; }
        public RouteViewModel Route { get; }

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

        public ICommand SaveRouteCommand { get; }
        public ICommand OpenRouteCommand { get; }
        public ICommand ClearRouteCommand { get; }
        public ICommand SelectSegmentCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand OpenLinkCommand { get; set; }
        public ICommand ResetWorldCommand { get; }

        public LandingPageViewModel LandingPageViewModel { get; }

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

        public bool ShowClimbs
        {
            get => _showClimbs;
            set
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
            set
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
            set
            {
                if (value == _showElevationProfile) return;
                _showElevationProfile = value;
                _userPreferences.ShowElevationProfile = _showElevationProfile;
                _userPreferences.Save();
                this.RaisePropertyChanged();
            }
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

        private async Task<CommandResult> OpenRoute()
        {
            if (Route.IsTainted)
            {
                MessageBoxResult questionResult = await _windowService.ShowShouldSaveRouteDialog();

                if (questionResult == MessageBoxResult.Cancel)
                {
                    return CommandResult.Aborted();
                }

                if (questionResult == MessageBoxResult.Yes)
                {
                    var saveResult = await SaveRoute();

                    // If saving was not successful then return the
                    // result of SaveRoute instead of proceeding.
                    if (saveResult.Result != Result.Success)
                    {
                        return saveResult;
                    }
                }
            }

            var (plannedRoute, fileName) = await _windowService.ShowOpenRouteDialog();

            if (plannedRoute != null)
            {
                Route.LoadFromPlannedRoute(plannedRoute);
                
                return CommandResult.Success();
            }

            if (fileName != null)
            {
                Route.OutputFilePath = fileName.EndsWith(".gpx")
                    ? Path.ChangeExtension(fileName, ".json")
                    : fileName;

                _userPreferences.LastUsedFolder = Path.GetDirectoryName(Route.OutputFilePath);
                _userPreferences.Save();

                SelectedSegment = null;

                try
                {
                    string? successMessage = null;

                    if (fileName.EndsWith(".gpx"))
                    {
                        var convertedRoute = _convertUseCase.Execute(ZwiftMapRoute.FromGpxFile(fileName));
                        Route.LoadFromPlannedRoute(convertedRoute, true);
                        successMessage = $"Successfully imported ZwiftMap route: {Path.GetFileName(fileName)}";
                    }
                    else
                    {
                        Route.Load();
                    }

                    this.RaisePropertyChanged(nameof(Route));

                    return successMessage == null
                        ? CommandResult.Success()
                        : CommandResult.SuccessWithMessage(successMessage);
                }
                catch (Exception e)
                {
                    return CommandResult.Failure(e.Message);
                }
            }

            return CommandResult.Aborted();
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

        private CommandResult OpenLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var startInfo = new ProcessStartInfo(uri.ToString())
                {
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                return CommandResult.Success();
            }

            return CommandResult.Failure("Invalid url");
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

        public string Version
        {
            get => _version ?? "0.0.0.0";
            set
            {
                if (value == _version) return;
                _version = value;
                ChangelogUri =
                    $"https://github.com/sandermvanvliet/RoadCaptain/blob/main/Changelog.md/#{Version.Replace(".", "")}";
                this.RaisePropertyChanged();
            }
        }

        public string? ChangelogUri
        {
            get => _changelogUri;
            set
            {
                if (value == _changelogUri) return;
                _changelogUri = value;
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

        public RouteSegmentListViewModel RouteSegmentListViewModel { get; }

        public async Task CheckForNewVersion()
        {
            if (_haveCheckedVersion)
            {
                return;
            }

            _haveCheckedVersion = true;

            var currentVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            if (latestRelease.official != null && latestRelease.official.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease.official);
            }
            else if (_applicationFeatures.IsPreRelease && 
                     latestRelease.preRelease != null &&
                     latestRelease.preRelease.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease.preRelease);
            }
        }

        public async Task CheckLastOpenedVersion()
        {
            if (_haveCheckedLastOpenedVersion)
            {
                return;
            }

            _haveCheckedLastOpenedVersion = true;

            var previousVersion = _userPreferences.LastOpenedVersion;
            var thisVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            // If there is a newer version available don't display anything
            if (latestRelease.official != null && latestRelease.official.Version > thisVersion)
            {
                return;
            }

            if (_applicationFeatures.IsPreRelease &&
                latestRelease.preRelease != null &&
                latestRelease.preRelease.Version > thisVersion)
            {
                return;
            }

            // If this version is newer than the previous one shown and it's
            // the latest version then show the what is new dialog
            if (latestRelease.official != null && thisVersion > previousVersion && thisVersion == latestRelease.official.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease.official);
            }
            else if (latestRelease.preRelease != null && thisVersion > previousVersion && thisVersion == latestRelease.preRelease.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease.preRelease);
            }
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
    }
}