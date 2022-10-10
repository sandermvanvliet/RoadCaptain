// Copyright (c) 2022 Sander van Vliet
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
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs;
using CommandResult = RoadCaptain.App.Shared.Commands.CommandResult;
using RelayCommand = RoadCaptain.App.Shared.Commands.RelayCommand;
using Result = RoadCaptain.App.Shared.Commands.Result;
using RoadCaptain.Ports;
using SkiaSharp;

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
        private readonly IWorldStore _worldStore;
        private readonly IUserPreferences _userPreferences;
        private WorldViewModel[] _worlds;
        private SportViewModel[] _sports;
        private List<Segment> _markers;
        private bool _showClimbs;
        private bool _showSprints;
        private Segment? _highlightedSegment;
        private bool _haveCheckedLastOpenedVersion;
        private bool _showElevationPlot;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker, IWindowService windowService, IWorldStore worldStore, IUserPreferences userPreferences)
        {
            _segments = new List<Segment>();
            _versionChecker = versionChecker;
            _windowService = windowService;
            _worldStore = worldStore;
            _userPreferences = userPreferences;

            Model = new MainWindowModel();
            _worlds = worldStore.LoadWorlds().Select(world => new WorldViewModel(world)).ToArray();
            _sports = new[] { new SportViewModel(SportType.Cycling), new SportViewModel(SportType.Running) };
            _markers = new List<Segment>();
            Route = new RouteViewModel(routeStore, segmentStore);

            Route.PropertyChanged += (_, args) => HandleRoutePropertyChanged(segmentStore, args);

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
                .OnFailure(_ => Model.StatusBarError("Failed to load route because: {0}", _.Message));

            ClearRouteCommand = new AsyncRelayCommand(
                    _ => ClearRoute(),
                    _ => Route.ReadyToBuild && Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .SubscribeTo(this, () => Route.ReadyToBuild)
                .OnSuccess(_ => Model.StatusBarInfo("Route cleared"))
                .OnFailure(_ => Model.StatusBarError("Failed to clear route because: {0}", _.Message));

            SelectSegmentCommand = new AsyncRelayCommand(
                    _ => SelectSegment(_ as Segment ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Added segment"))
                .OnSuccessWithMessage(_ => Model.StatusBarInfo("Added segment {0}", _.Message))
                .OnFailure(_ => Model.StatusBarWarning(_.Message));

            RemoveLastSegmentCommand = new RelayCommand(
                    _ => RemoveLastSegment(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence)
                .OnSuccess(_ =>
                {
                    Model.StatusBarInfo("Removed segment");
                })
                .OnSuccessWithMessage(_ =>
                {
                    Model.StatusBarInfo("Removed segment {0}", _.Message);
                })
                .OnFailure(_ => Model.StatusBarWarning(_.Message));

            SimulateCommand = new RelayCommand(
                    _ => SimulateRoute(),
                    _ => Route.Sequence.Any())
                .SubscribeTo(this, () => Route.Sequence);

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => !string.IsNullOrEmpty(_ as string));

            SelectWorldCommand = new RelayCommand(
                _ => SelectWorld(_ as WorldViewModel ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => (_ as WorldViewModel)?.CanSelect ?? false);

            SelectSportCommand = new AsyncRelayCommand(
                _ => SelectSport(_ as SportViewModel ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => _ is SportViewModel);

            ResetDefaultSportCommand = new RelayCommand(
                _ => ResetDefaultSport(),
                _ => true);

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

            SimulationState = SimulationState.NotStarted;

            Segments = new List<Segment>();
            SegmentPaths.Clear();

            Markers = new();

            var selectedSport = Sports.SingleOrDefault(s => s.IsSelected);
            if (selectedSport != null)
            {
                selectedSport.IsSelected = false;

                SelectDefaultSportFromPreferences();
            }

            var selectedWorld = Worlds.SingleOrDefault(s => s.IsSelected);
            if (selectedWorld != null)
            {
                selectedWorld.IsSelected = false;
            }

            return CommandResult.Success();
        }

        private CommandResult ResetDefaultSport()
        {
            _userPreferences.DefaultSport = null;
            _userPreferences.Save();

            foreach (var sport in _sports)
            {
                sport.IsDefault = false;
            }

            DefaultSport = null;

            return CommandResult.Success();
        }

        private void SelectDefaultSportFromPreferences()
        {
            if (HasDefaultSport)
            {
                var sport = Sports
                    .SingleOrDefault(s =>
                        (_userPreferences.DefaultSport ?? "").Equals(s.Sport.ToString(), StringComparison.InvariantCultureIgnoreCase));

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

            this.RaisePropertyChanged(nameof(Route));
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
        public RouteViewModel Route { get; set; }
        public Dictionary<string, SKPath> SegmentPaths { get; } = new();

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

                this.RaisePropertyChanged(nameof(Markers));
            }
        }

        public ICommand SaveRouteCommand { get; }
        public ICommand OpenRouteCommand { get; }
        public ICommand ClearRouteCommand { get; }
        public ICommand SelectSegmentCommand { get; }
        public ICommand RemoveLastSegmentCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand OpenLinkCommand { get; set; }
        public ICommand SelectWorldCommand { get; }
        public ICommand SelectSportCommand { get; }
        public ICommand ResetDefaultSportCommand { get; }
        public ICommand ResetWorldCommand { get; }

        public Segment? SelectedSegment
        {
            get => _selectedSegment;
            private set
            {
                if (value == _selectedSegment) return;
                _selectedSegment = value;
                this.RaisePropertyChanged(nameof(SelectedSegment));
            }
        }

        public bool HasDefaultSport => !string.IsNullOrEmpty(DefaultSport);

        public string? DefaultSport
        {
            get => _userPreferences.DefaultSport;
            private set
            {
                _userPreferences.DefaultSport = value;
                _userPreferences.Save();

                this.RaisePropertyChanged(nameof(DefaultSport));
                this.RaisePropertyChanged(nameof(HasDefaultSport));
            }
        }

        public bool ShowClimbs
        {
            get => _showClimbs;
            set
            {
                if (value == _showClimbs) return;
                _showClimbs = value;
                this.RaisePropertyChanged(nameof(ShowClimbs));
            }
        }

        public bool ShowSprints
        {
            get => _showSprints;
            set
            {
                if (value == _showSprints) return;
                _showSprints = value;
                this.RaisePropertyChanged(nameof(ShowSprints));
            }
        }

        public bool ShowElevationPlot
        {
            get => _showElevationPlot;
            set
            {
                if (value == _showElevationPlot) return;
                _showElevationPlot = value;
                this.RaisePropertyChanged(nameof(ShowElevationPlot));
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
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

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
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

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

                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

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

                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

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
            var result = Route.IsPossibleLoop();

            if (result.Item1)
            {
                var shouldCreateLoop = await _windowService.ShowRouteLoopDialog();

                if (shouldCreateLoop)
                {
                    Route.MakeLoop(result.Item2.Value, result.Item3.Value);
                }
            }
        }

        private bool IsValidSpawnPointProgression(out CommandResult commandResult, SegmentDirection segmentDirection)
        {
            if (Route.World != null && Route.Sequence.Count() == 1)
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

        private static SegmentDirection GetDirectionOnNewSegment(Segment newSelectedSegment, Segment lastSegment)
        {
            var newSegmentDirection = SegmentDirection.Unknown;

            if (newSelectedSegment.NextSegmentsNodeA.Any(s => s.SegmentId == lastSegment.Id))
            {
                newSegmentDirection = SegmentDirection.AtoB;
            }
            else if (newSelectedSegment.NextSegmentsNodeB.Any(s => s.SegmentId == lastSegment.Id))
            {
                newSegmentDirection = SegmentDirection.BtoA;
            }

            return newSegmentDirection;
        }

        private async Task<CommandResult> ClearRoute()
        {
            var result = await _windowService.ShowClearRouteDialog();

            if (result == MessageBoxResult.No)
            {
                return CommandResult.Aborted();
            }

            var commandResult = Route.Clear();

            SelectedSegment = null;
            HighlightedSegment = null;

            SimulationState = SimulationState.NotStarted;

            return commandResult;
        }

        private async Task<CommandResult> SaveRoute()
        {
            var outputFilePath = await _windowService.ShowSaveRouteDialog(_userPreferences.LastUsedFolder, Route);

            if (!string.IsNullOrEmpty(outputFilePath))
            {
                _userPreferences.LastUsedFolder = Path.GetDirectoryName(outputFilePath);
                _userPreferences.Save();

                return CommandResult.Success();
            }

            return CommandResult.Aborted();
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

            var fileName = await _windowService.ShowOpenFileDialog(_userPreferences.LastUsedFolder);

            if (string.IsNullOrEmpty(fileName))
            {
                return CommandResult.Success();
            }

            Route.OutputFilePath = fileName;

            _userPreferences.LastUsedFolder = Path.GetDirectoryName(Route.OutputFilePath);
            _userPreferences.Save();

            SelectedSegment = null;

            try
            {
                Route.Load();

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
        }

        private CommandResult SelectWorld(WorldViewModel world)
        {
            Route.World = _worldStore.LoadWorldById(world.Id);

            var currentSelected = Worlds.SingleOrDefault(w => w.IsSelected);
            if (currentSelected != null)
            {
                currentSelected.IsSelected = false;
            }

            world.IsSelected = true;

            return CommandResult.Success();
        }

        private async Task<CommandResult> SelectSport(SportViewModel sport)
        {
            Route.Sport = sport.Sport;

            if (string.IsNullOrEmpty(_userPreferences.DefaultSport))
            {
                var result = await _windowService.ShowDefaultSportSelectionDialog(sport.Sport);

                if (result)
                {
                    DefaultSport = sport.Sport.ToString();
                    sport.IsDefault = true;
                }
            }

            var currentSelected = Sports.SingleOrDefault(w => w.IsSelected);
            if (currentSelected != null)
            {
                currentSelected.IsSelected = false;
            }

            sport.IsSelected = true;

            return CommandResult.Success();
        }

        private CommandResult SimulateRoute()
        {
            if (_simulationTask != null && SimulationState == SimulationState.Running)
            {
                SimulationState = SimulationState.Completed;
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
                    Thread.Sleep(15);
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
                this.RaisePropertyChanged(nameof(RiderPosition));
            }
        }

        public SimulationState SimulationState
        {
            get => _simulationState;
            set
            {
                _simulationState = value;
                this.RaisePropertyChanged(nameof(SimulationState));
            }
        }

        public string Version
        {
            get => _version ?? "0.0.0.0";
            set
            {
                if (value == _version) return;
                _version = value;
                ChangelogUri = $"https://github.com/sandermvanvliet/RoadCaptain/blob/main/Changelog.md/#{Version.Replace(".", "")}";
                this.RaisePropertyChanged(nameof(Version));
            }
        }

        public string? ChangelogUri
        {
            get => _changelogUri;
            set
            {
                if (value == _changelogUri) return;
                _changelogUri = value;
                this.RaisePropertyChanged(nameof(ChangelogUri));
            }
        }

        public WorldViewModel[] Worlds
        {
            get => _worlds;
            set
            {
                if (value == _worlds) return;
                _worlds = value;
                this.RaisePropertyChanged(nameof(Worlds));
            }
        }

        public SportViewModel[] Sports
        {
            get => _sports;
            set
            {
                if (value == _sports) return;
                _sports = value;
                this.RaisePropertyChanged(nameof(Sports));
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
                this.RaisePropertyChanged(nameof(HighlightedSegment));
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
                this.RaisePropertyChanged(nameof(Segments));
            }
        }

        public async Task CheckForNewVersion()
        {
            if (_haveCheckedVersion)
            {
                return;
            }

            _haveCheckedVersion = true;

            var currentVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            if (latestRelease != null && latestRelease.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease);
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
            if (latestRelease != null && latestRelease.Version > thisVersion)
            {
                return;
            }

            // If this version is newer than the previous one shown and it's
            // the latest version then show the what is new dialog
            if (latestRelease != null && thisVersion > previousVersion && thisVersion == latestRelease.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease);
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

        public void ClearSegmentHighlight()
        {
            HighlightedSegment = null;
        }
    }
}

