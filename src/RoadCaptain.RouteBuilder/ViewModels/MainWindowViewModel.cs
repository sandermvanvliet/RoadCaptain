using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.Ports;
using RoadCaptain.UserInterface.Shared.Commands;
using RoadCaptain.RouteBuilder.Models;
using SkiaSharp;
using Point = System.Windows.Point;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private Segment _selectedSegment;
        private readonly Dictionary<string, SKRect> _segmentPathBounds = new();
        private List<Segment> _segments;
        private Task _simulationTask;
        private SKPoint? _riderPosition;
        private SimulationState _simulationState = SimulationState.NotStarted;
        private int _simulationIndex;
        private Offsets _overallOffsets;
        private string _version;
        private string _changelogUri;
        private bool _haveCheckedVersion;
        private readonly IVersionChecker _versionChecker;
        private readonly IWindowService _windowService;
        private readonly IWorldStore _worldStore;
        private readonly UserPreferences _userPreferences;
        private WorldViewModel[] _worlds;
        private SportViewModel[] _sports;
        private List<Segment> _markers;
        private bool _showClimbs;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker, IWindowService windowService, IWorldStore worldStore, UserPreferences userPreferences)
        {
            _segments = new List<Segment>();
            _versionChecker = versionChecker;
            _windowService = windowService;
            _worldStore = worldStore;
            _userPreferences = userPreferences;
            Model = new MainWindowModel();
            Worlds = worldStore.LoadWorlds().Select(world => new WorldViewModel(world)).ToArray();
            Sports = new[] { new SportViewModel(SportType.Cycling), new SportViewModel(SportType.Running) };
            Route = new RouteViewModel(routeStore, segmentStore);
            
            Route.PropertyChanged += (_, args) => HandleRoutePropertyChanged(segmentStore, args);
            
            SelectDefaultSportFromPreferences();

            SaveRouteCommand = new RelayCommand(
                    _ => SaveRoute(),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Route saved successfully"))
                .OnSuccessWithWarnings(_ => Model.StatusBarInfo("Route saved successfully: {0}", _.Message))
                .OnFailure(_ => Model.StatusBarError("Failed to save route because: {0}", _.Message))
                .OnNotExecuted(_ => Model.StatusBarInfo("Route hasn't changed dit not need to not saved"));

            OpenRouteCommand = new RelayCommand(
                    _ => OpenRoute(),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Route loaded successfully"))
                .OnFailure(_ => Model.StatusBarError("Failed to load route because: {0}", _.Message));

            ResetRouteCommand = new RelayCommand(
                    _ => ResetRoute(),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Route reset"))
                .OnFailure(_ => Model.StatusBarError("Failed to reset route because: {0}", _.Message));

            SelectSegmentCommand = new RelayCommand(
                    _ => SelectSegment((Point)_),
                    _ => true)
                .OnSuccess(_ => Model.StatusBarInfo("Added segment"))
                .OnSuccessWithWarnings(_ => Model.StatusBarInfo("Added segment {0}", _.Message))
                .OnFailure(_ => Model.StatusBarWarning(_.Message));

            RemoveLastSegmentCommand = new RelayCommand(
                _ => RemoveLastSegment(),
                _ => Route.Sequence.Any())
                    .OnSuccess(_ =>
                    {
                        Model.StatusBarInfo("Removed segment");
                    })
                    .OnSuccessWithWarnings(_ =>
                    {
                        Model.StatusBarInfo("Removed segment {0}", _.Message);
                    })
                    .OnFailure(_ => Model.StatusBarWarning(_.Message));

            SimulateCommand = new RelayCommand(
                    _ => SimulateRoute(),
                    _ => true);

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string),
                _ => !string.IsNullOrEmpty(_ as string));

            SelectWorldCommand = new RelayCommand(
                _ => SelectWorld(_ as WorldViewModel),
                _ => (_ as WorldViewModel)?.CanSelect ?? false);
            
            SelectSportCommand = new RelayCommand(
                _ => SelectSport(_ as SportViewModel),
                _ => true);

            ResetDefaultSportCommand = new RelayCommand(
                _ => ResetDefaultSport(), 
                _ => true);

            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";
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
                        _userPreferences.DefaultSport.Equals(s.Sport.ToString(), StringComparison.InvariantCultureIgnoreCase));

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
            if (args.PropertyName == nameof(Route.Sequence))
            {
                if (Route.Sequence.Any())
                {
                    var points = SegmentPaths[Route.Last.SegmentId].Points;

                    if (Route.Sequence.Last().Direction == SegmentDirection.BtoA)
                    {
                        points = points.Reverse().ToArray();
                    }

                    RoutePath.AddPoly(points, false);
                }
            }

            if (args.PropertyName == nameof(Route.World))
            {
                if (Route.World == null)
                {
                    _segments = new List<Segment>();
                }

                TryLoadSegmentsForWorldAndSport(segmentStore);
            }

            if (args.PropertyName == nameof(Route.Sport))
            {
                if (Route.Sport == SportType.Unknown)
                {
                    _segments = new List<Segment>();
                }
                
                TryLoadSegmentsForWorldAndSport(segmentStore);
            }

            OnPropertyChanged(nameof(Route));
        }

        private void TryLoadSegmentsForWorldAndSport(ISegmentStore segmentStore)
        {
            if (Route.World != null && Route.Sport != SportType.Unknown)
            {
                _segments = segmentStore.LoadSegments(Route.World, Route.Sport);
                _markers = segmentStore.LoadMarkers(Route.World);
            }
        }

        public MainWindowModel Model { get; }
        public RouteViewModel Route { get; set; }
        public Dictionary<string, SKPath> SegmentPaths { get; } = new();
        public Dictionary<string, Marker> Markers { get; } = new();
        public SKPath RoutePath { get; private set; } = new SKPath();

        public ICommand SaveRouteCommand { get; }
        public ICommand OpenRouteCommand { get; }
        public ICommand ResetRouteCommand { get; }
        public ICommand SelectSegmentCommand { get; }
        public ICommand RemoveLastSegmentCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand OpenLinkCommand { get; set; }
        public ICommand SelectWorldCommand { get; }
        public ICommand SelectSportCommand { get; }
        public ICommand ResetDefaultSportCommand { get; }

        public Segment SelectedSegment
        {
            get => _selectedSegment;
            private set
            {
                if (value == _selectedSegment) return;
                _selectedSegment = value;
                OnPropertyChanged();
            }
        }

        public bool HasDefaultSport
        {
            get => !string.IsNullOrEmpty(DefaultSport);
        }

        public string DefaultSport
        {
            get => _userPreferences.DefaultSport;
            private set
            {
                _userPreferences.DefaultSport = value;
                _userPreferences.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDefaultSport));
            }
        }

        public bool ShowClimbs
        {
            get => _showClimbs;
            set
            {
                if(value == _showClimbs) return;
                _showClimbs = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private CommandResult SelectSegment(Point scaledPoint)
        {
            // Find SKPath that contains this coordinate (or close enough)
            var pathsInBounds = _segmentPathBounds
                .Where(p => p.Value.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
                .OrderBy(x => x.Value, new SkRectComparer()) // Sort by bounds area, good enough for now
                .ToList();

            if (!pathsInBounds.Any())
            {
                return CommandResult.Aborted();
            }

            // Do expensive point to segment matching now that we've narrowed down the set
            var boundedSegments = pathsInBounds.Select(kv => _segments.Single(s => s.Id == kv.Key)).ToList();

            var reverseScaled = _overallOffsets.ReverseScaleAndTranslate(scaledPoint.X, scaledPoint.Y);
            var scaledPointToPosition = TrackPoint.FromGameLocation(reverseScaled.Latitude, reverseScaled.Longitude, reverseScaled.Altitude);
            scaledPointToPosition = new TrackPoint(-scaledPointToPosition.Longitude, scaledPointToPosition.Latitude, scaledPointToPosition.Altitude);

            Segment newSelectedSegment = null;

            foreach (var segment in boundedSegments)
            {
                if (segment.Contains(scaledPointToPosition))
                {
                    newSelectedSegment = segment;
                }
            }

            newSelectedSegment ??= boundedSegments.First();

            var segmentId = newSelectedSegment.Id;

            // 1. Figure out if this is the first segment on the route, if so add it to the route and set the selection to the new segment
            if (!Route.Sequence.Any())
            {
                if (!Route.IsSpawnPointSegment(segmentId))
                {
                    return CommandResult.Failure($"{newSelectedSegment.Name} is not a spawn point, we can't start here unfortunately");
                }

                Route.StartOn(newSelectedSegment);

                SelectedSegment = newSelectedSegment;

                return CommandResult.SuccessWithWarning(newSelectedSegment.Name);
            }

            // Prevent selecting the same segment again
            if (Route.Last.SegmentId == segmentId)
            {
                return CommandResult.Aborted();
            }

            if (!string.IsNullOrEmpty(newSelectedSegment.NoSelectReason))
            {
                return CommandResult.Failure(newSelectedSegment.NoSelectReason);
            }

            // 2. Figure out if the newly selected segment is reachable from the last segment
            var lastSegment = _segments.Single(s => s.Id == Route.Last.SegmentId);

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

                    return CommandResult.SuccessWithWarning(newSelectedSegment.Name);
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

                    return CommandResult.SuccessWithWarning(newSelectedSegment.Name);
                }
            }
            else if (Route.Last.Direction == SegmentDirection.Unknown)
            {
                if (fromA != null)
                {
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

                    Route.NextStep(fromA.Direction, fromA.SegmentId, newSelectedSegment, SegmentDirection.BtoA,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    return CommandResult.SuccessWithWarning(newSelectedSegment.Name);
                }

                if (fromB != null)
                {
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

                    Route.NextStep(fromB.Direction, fromB.SegmentId, newSelectedSegment, SegmentDirection.AtoB,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    return CommandResult.SuccessWithWarning(newSelectedSegment.Name);
                }
            }

            return CommandResult.Failure("Did not find a connection between the last segment and the selected segment");
        }

        private CommandResult RemoveLastSegment()
        {
            if (!Route.Sequence.Any())
            {
                return CommandResult.Failure("Can't remove segment because the route does not have any segments");
            }

            var lastSegment = Route.RemoveLast();
            
            SelectedSegment = null;
            CreateRoutePath();

            return CommandResult.SuccessWithWarning(lastSegment.SegmentName);
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

        public void CreatePathsForSegments(float width, float height)
        {
            SegmentPaths.Clear();
            _segmentPathBounds.Clear();

            if (_segments == null || !_segments.Any())
            {
                return;
            }

            var segmentsWithOffsets = _segments
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point =>
                        TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude)).ToList()
                })
                .Select(x => new
                {
                    x.Segment,
                    x.GameCoordinates,
                    Offsets = new Offsets(width, height, x.GameCoordinates)
                })
                .ToList();

            _overallOffsets = Offsets.From(segmentsWithOffsets.Select(s => s.Offsets).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                var skiaPathFromSegment = SkiaPathFromSegment(_overallOffsets, segment.GameCoordinates);
                skiaPathFromSegment.GetTightBounds(out var bounds);

                SegmentPaths.Add(segment.Segment.Id, skiaPathFromSegment);
                _segmentPathBounds.Add(segment.Segment.Id, bounds);
            }

            CreateRoutePath();

            CreateMarkers();
        }

        private void CreateMarkers()
        {
            Markers.Clear();

            if (_markers == null || !_markers.Any())
            {
                return;
            }

            foreach (var segment in _markers.Where(m => m.Type == SegmentType.Climb))
            {
                var firstPoint = segment.Points.First();
                var lastPoint = segment.Points.Last();
                var startPoint = _overallOffsets.ScaleAndTranslate(TrackPoint.LatLongToGame(firstPoint.Longitude, -firstPoint.Latitude, firstPoint.Altitude));
                var endPoint = _overallOffsets.ScaleAndTranslate(TrackPoint.LatLongToGame(lastPoint.Longitude, -lastPoint.Latitude, lastPoint.Altitude));

                var marker = new Marker
                {
                    Id = segment.Id,
                    StartPoint = new SKPoint(startPoint.X, startPoint.Y),
                    EndPoint = new SKPoint(endPoint.X, endPoint.Y),
                    StartAngle = (float)TrackPoint.Bearing(segment.Points[0], segment.Points[1]) + 90,
                    EndAngle = (float)TrackPoint.Bearing(segment.Points[^2], segment.Points[^1]) + 90
                };

                Markers.Add(segment.Id, marker);
            }
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<TrackPoint> data)
        {
            var path = new SKPath();

            path.AddPoly(
                data
                    .Select(offsets.ScaleAndTranslate)
                    .Select(point => new SKPoint(point.X, point.Y))
                    .ToArray(),
                false);

            return path;
        }

        private CommandResult ResetRoute()
        {
            var commandResult = Route.Reset();

            SelectedSegment = null;

            RoutePath = new SKPath();
            SimulationState = SimulationState.NotStarted;
            _simulationIndex = 0;
            
            _segments = null;
            SegmentPaths.Clear();

            _markers = null;
            Markers.Clear();

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

            return commandResult;
        }

        private CommandResult SaveRoute()
        {
            var routeOutputFilePath = _windowService.ShowSaveFileDialog();

            if (string.IsNullOrEmpty(routeOutputFilePath))
            {
                return CommandResult.Success();
            }

            Route.OutputFilePath = routeOutputFilePath;

            try
            {
                Route.Save();
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
        }

        private CommandResult OpenRoute()
        {
            if (Route.IsTainted)
            {
                // First save
                var questionResult = MessageBox.Show(
                    "Do you want to save the current route?",
                    "Current route was changed",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information);

                if (questionResult == MessageBoxResult.Cancel)
                {
                    return CommandResult.Aborted();
                }

                if (questionResult == MessageBoxResult.Yes)
                {
                    var saveResult = SaveRoute();

                    // If saving was not successful then return the
                    // result of SaveRoute instead of proceeding.
                    if (saveResult.Result != Result.Success)
                    {
                        return saveResult;
                    }
                }
            }

            var fileName = _windowService.ShowOpenFileDialog();

            if (string.IsNullOrEmpty(fileName))
            {
                return CommandResult.Success();
            }

            Route.OutputFilePath = fileName;

            SelectedSegment = null;

            try
            {
                Route.Load();

                CreateRoutePath();

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

        private CommandResult SelectSport(SportViewModel sport)
        {
            Route.Sport = sport.Sport;

            if (string.IsNullOrEmpty(_userPreferences.DefaultSport))
            {
                var result = _windowService.ShowDefaultSportSelectionDialog(sport.Sport);

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

        private void CreateRoutePath()
        {
            RoutePath = new SKPath();

            // RoutePath needs to be set to the total route we just loaded
            foreach (var segment in Route.Sequence)
            {
                var points = SegmentPaths[segment.SegmentId].Points;

                if (segment.Direction == SegmentDirection.BtoA)
                {
                    points = points.Reverse().ToArray();
                }

                RoutePath.AddPoly(points, false);
            }
        }

        private CommandResult SimulateRoute()
        {
            if (_simulationTask != null && SimulationState == SimulationState.Running)
            {
                SimulationState = SimulationState.Paused;
                return CommandResult.Success();
            }

            _simulationTask = Task.Factory.StartNew(() =>
            {
                SimulationState = SimulationState.Running;

                while (SimulationState == SimulationState.Running && _simulationIndex < RoutePath.PointCount)
                {
                    RiderPosition = RoutePath.Points[_simulationIndex++];
                    Thread.Sleep(15);
                }

                if (SimulationState != SimulationState.Paused)
                {
                    _simulationIndex = 0;
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

        public SKPoint? RiderPosition
        {
            get => _riderPosition;
            set
            {
                _riderPosition = value;
                OnPropertyChanged();
            }
        }

        public SimulationState SimulationState
        {
            get => _simulationState;
            set
            {
                _simulationState = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                ChangelogUri = $"https://github.com/sandermvanvliet/RoadCaptain/blob/main/Changelog.md/#{Version.Replace(".", "")}";
                OnPropertyChanged();
            }
        }

        public string ChangelogUri
        {
            get => _changelogUri;
            set
            {
                if (value == _changelogUri) return;
                _changelogUri = value;
                OnPropertyChanged();
            }
        }

        public WorldViewModel[] Worlds
        {
            get => _worlds;
            set
            {
                if (value == _worlds) return;
                _worlds = value;
                OnPropertyChanged();
            }
        }

        public SportViewModel[] Sports
        {
            get => _sports;
            set
            {
                if (value == _sports) return;
                _sports = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CheckForNewVersion()
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
                _windowService.ShowNewVersionDialog(latestRelease);
            }
        }
    }

    public class Marker
    {
        public string Id { get; set; }
        public SKPoint StartPoint { get; set; }
        public SKPoint EndPoint { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
    }

    public enum SimulationState
    {
        NotStarted,
        Running,
        Paused,
        Completed
    }
}