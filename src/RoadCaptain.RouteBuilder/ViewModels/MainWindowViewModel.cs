using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RoadCaptain.Ports;
using RoadCaptain.RouteBuilder.Annotations;
using RoadCaptain.RouteBuilder.Commands;
using RoadCaptain.RouteBuilder.Models;
using SkiaSharp;
using Point = System.Windows.Point;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private Segment _selectedSegment;
        private readonly Dictionary<string, SKRect> _segmentPathBounds = new();
        private readonly List<Segment> _segments;
        private Task _simulationTask;
        private SKPoint? _riderPosition;
        private SimulationState _simulationState = SimulationState.NotStarted;
        private int _simulationIndex;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            Model = new MainWindowModel();

            Route = new RouteViewModel(routeStore, segmentStore);
            Route.PropertyChanged += (_, args) =>
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

                OnPropertyChanged(nameof(Route));
            };

            _segments = segmentStore.LoadSegments();

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

            SimulateCommnad = new RelayCommand(
                    _ => SimulateRoute(),
                    _ => true);
        }

        public MainWindowModel Model { get; }
        public RouteViewModel Route { get; set; }
        public Dictionary<string, SKPath> SegmentPaths { get; } = new();
        public SKPath RoutePath { get; private set; } = new SKPath();

        public ICommand SaveRouteCommand { get; }
        public ICommand OpenRouteCommand { get; }
        public ICommand ResetRouteCommand { get; }
        public ICommand SelectSegmentCommand { get; }
        public ICommand SimulateCommnad { get; }

        public Segment SelectedSegment
        {
            get => _selectedSegment;
            private set
            {
                _selectedSegment = value;
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

            var segmentId = pathsInBounds.First().Key;

            var newSelectedSegment = _segments.Single(s => s.Id == segmentId);

            // 1. Figure out if this is the first segment on the route, if so add it to the route and set the selection to the new segment
            if (!Route.Sequence.Any())
            {
                if (!Route.IsSpawnPointSegment(segmentId))
                {
                    return CommandResult.Failure($"{segmentId} is not a spawn point, we can't start here unfortunately");
                }

                Route.StartOn(newSelectedSegment);

                SelectedSegment = newSelectedSegment;

                return CommandResult.Success();
            }

            // Prevent selecting the same segment again
            if (Route.Last.SegmentId == segmentId)
            {
                return CommandResult.Aborted();
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

                    return CommandResult.SuccessWithWarning(segmentId);
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

                    return CommandResult.SuccessWithWarning(segmentId);
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

                    return CommandResult.SuccessWithWarning(segmentId);
                }

                if (fromB != null)
                {
                    var newSegmentDirection = GetDirectionOnNewSegment(newSelectedSegment, lastSegment);

                    Route.NextStep(fromB.Direction, fromB.SegmentId, newSelectedSegment, SegmentDirection.AtoB,
                        newSegmentDirection);

                    SelectedSegment = newSelectedSegment;

                    return CommandResult.SuccessWithWarning(segmentId);
                }
            }

            return CommandResult.Failure("Did not find a connection between the last segment and the selected segment");
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

        public void CreatePathsForSegments(float width)
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
                    Offsets = new Offsets(width, x.GameCoordinates)
                })
                .ToList();

            var overallOffsets = Offsets.From(segmentsWithOffsets.Select(s => s.Offsets).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                var skiaPathFromSegment = SkiaPathFromSegment(overallOffsets, segment.GameCoordinates);
                skiaPathFromSegment.GetTightBounds(out var bounds);

                SegmentPaths.Add(segment.Segment.Id, skiaPathFromSegment);
                _segmentPathBounds.Add(segment.Segment.Id, bounds);
            }

            CreateRoutePath();
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

            return commandResult;
        }

        private CommandResult SaveRoute()
        {
            if (!Route.IsTainted)
            {
                return CommandResult.Aborted();
            }

            if (string.IsNullOrEmpty(Route.OutputFilePath))
            {
                var dialog = new SaveFileDialog
                {
                    RestoreDirectory = true,
                    AddExtension = true,
                    DefaultExt = ".json",
                    Filter = "JSON files (.json)|*.json"
                };

                var result = dialog.ShowDialog();

                if (!result.HasValue || !result.Value)
                {
                    return CommandResult.Success();
                }

                Route.OutputFilePath = dialog.FileName;
            }

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

            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "JSON files (.json)|*.json",
                Multiselect = false
            };

            var result = dialog.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                return CommandResult.Success();
            }

            Route.OutputFilePath = dialog.FileName;

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
            });

            return CommandResult.Success();
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

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SimulationState
    {
        NotStarted,
        Running,
        Paused,
        Completed
    }
}