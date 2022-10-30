// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;
using ListBox = Avalonia.Controls.ListBox;
using Point = Avalonia.Point;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MainWindow : Window
    {
        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            ViewModel = DataContext as MainWindowViewModel;

            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IUserPreferences userPreferences)
        {
            this.UseWindowStateTracking(
                userPreferences.RouteBuilderLocation,
                newWindowLocation =>
                {
                    userPreferences.RouteBuilderLocation = newWindowLocation;
                    userPreferences.Save();
                });

            ViewModel = viewModel;
            ViewModel.PropertyChanged += WindowViewModelPropertyChanged;
            DataContext = viewModel;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            ZwiftMap.RenderPriority = new ZwiftMapRenderPriority();

            var modifier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? KeyModifiers.Meta
                : KeyModifiers.Control;

            KeyBindings.Add(new KeyBinding
            { Command = ViewModel.OpenRouteCommand, Gesture = new KeyGesture(Key.O, modifier) });
            KeyBindings.Add(new KeyBinding
            { Command = ViewModel.SaveRouteCommand, Gesture = new KeyGesture(Key.S, modifier) });
            KeyBindings.Add(new KeyBinding
            { Command = ViewModel.ClearRouteCommand, Gesture = new KeyGesture(Key.R, modifier) });
            KeyBindings.Add(new KeyBinding
            { Command = ViewModel.RemoveLastSegmentCommand, Gesture = new KeyGesture(Key.Z, modifier) });
        }

        private MainWindowViewModel ViewModel { get; }

        private void WindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedSegment):
                case nameof(ViewModel.SegmentPaths):
                    // Reset any manually selected item in the list
                    using (ZwiftMap.BeginUpdate())
                    {
                        ViewModel.ClearSegmentHighlight();

                        SynchronizeRouteSegmentsOnZwiftMap();
                    }
                    break;
                case nameof(ViewModel.HighlightedSegment):
                    HighlightOnZwiftMap();
                    break;
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    using (ZwiftMap.BeginUpdate())
                    {
                        SetZwiftMap();
                        SynchronizeRouteSegmentsOnZwiftMap();
                    }

                    break;
                case nameof(ViewModel.RiderPosition):
                    var routePath = ZwiftMap.MapObjects.OfType<RoutePath>().SingleOrDefault();

                    if (routePath != null)
                    {
                        if (ViewModel.RiderPosition == null)
                        {
                            routePath.Reset();
                            routePath.IsVisible = false;
                        }
                        else
                        {
                            routePath.IsVisible = true;
                            routePath.MoveNext();
                        }
                    }

                    InvalidateZwiftMap();

                    break;
                case nameof(ViewModel.ShowClimbs):
                    ToggleClimbs(ViewModel.ShowClimbs);
                    break;
                case nameof(ViewModel.ShowSprints):
                    ToggleSprints(ViewModel.ShowSprints);
                    break;
            }
        }

        private void ToggleSprints(bool visible)
        {
            ZwiftMap.MapObjects.OfType<SprintSegment>()
                .ToList()
                .ForEach(climb => climb.IsVisible = visible);

            InvalidateZwiftMap();
        }

        private void ToggleClimbs(bool visible)
        {
            ZwiftMap.MapObjects.OfType<ClimbSegment>()
                .ToList()
                .ForEach(climb => climb.IsVisible = visible);

            InvalidateZwiftMap();
        }

        private void InvalidateZwiftMap([CallerMemberName] string? caller = null)
        {
            Debug.WriteLine($"[InvalidateZwiftMap] {caller}");
            ZwiftMap.InvalidateVisual();
        }

        private void SynchronizeRouteSegmentsOnZwiftMap()
        {
            using var updateScope = ZwiftMap.BeginUpdate();

            var mapSegments = ZwiftMap
                .MapObjects
                .OfType<MapSegment>()
                .ToList();

            var routeHasSegments = ViewModel.Route.Sequence.Any();

            foreach (var spawnPoint in ZwiftMap.MapObjects.OfType<SpawnPointSegment>())
            {
                spawnPoint.IsVisible = !routeHasSegments;
            }

            foreach (var segment in mapSegments)
            {
                var seq = ViewModel.Route.Sequence.FirstOrDefault(s => s.SegmentId == segment.SegmentId);

                if (seq != null)
                {
                    segment.IsLeadIn = seq.Type == SegmentSequenceType.LeadIn;
                    segment.IsLeadOut = seq.Type == SegmentSequenceType.LeadOut;
                    segment.IsLoop = seq.Type == SegmentSequenceType.Loop || seq.Type == SegmentSequenceType.LoopEnd || seq.Type == SegmentSequenceType.LoopStart;
                    segment.IsOnRoute = true;
                }
                else if (segment.IsOnRoute)
                {
                    segment.IsOnRoute = false;
                }
            }

            // Synchronize the route path with the route
            SynchronizeRoutePath(mapSegments);
        }

        private void SynchronizeRoutePath(List<MapSegment> mapSegments)
        {
            var routePath = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is RoutePath);

            var routePathPoints = RoutePathPointsFrom(ViewModel.Route.Sequence, mapSegments);

            if (routePath != null)
            {
                ZwiftMap.MapObjects.Remove(routePath);
            }

            routePath = new RoutePath(routePathPoints);
            ZwiftMap.MapObjects.Add(routePath);
        }

        private static SKPoint[] RoutePathPointsFrom(IEnumerable<SegmentSequenceViewModel> routeSequence,
            List<MapSegment> mapSegments)
        {
            var routePoints = new List<SKPoint>();

            foreach (var seq in routeSequence)
            {
                var points = mapSegments.Single(s => s.SegmentId == seq.SegmentId).Points;

                if (seq.Direction == SegmentDirection.BtoA)
                {
                    // Don't call Reverse() because that does an
                    // in-place reverse and given that we're 
                    // _referencing_ the list of points of the
                    // segment that means that the actual segment
                    // is modified. Reverse() does not return a
                    // new IEnumerable<T>
                    points = points.AsEnumerable().Reverse().ToArray();
                }

                routePoints.AddRange(points);
            }

            return routePoints.ToArray();
        }

        private void HighlightOnZwiftMap()
        {
            var highlightedSegments = ZwiftMap
                .MapObjects
                .OfType<MapSegment>()
                .ToList();

            var highlightedSegmentId = (ViewModel.HighlightedSegment?.Id ?? "no selection");

            foreach (var segment in highlightedSegments)
            {
                if (segment.SegmentId == highlightedSegmentId)
                {
                    segment.IsHighlighted = true;
                }
                else
                {
                    segment.IsHighlighted = false;
                }
            }

            // Force re-render
            InvalidateZwiftMap();
        }

        private void AddPathsToMap(World world)
        {
            if (!ViewModel.Segments.Any())
            {
                return;
            }

            var map = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is WorldMap);

            if (map == null)
            {
                return;
            }

            var offsets = CreatePathsForSegments(ViewModel.Segments, world);

            CreatePathsForMarkers(
                offsets,
                ViewModel.Markers.Where(m => m.Type == SegmentType.Climb).ToList(),
                (id, points) => new ClimbSegment(id, points));

            CreatePathsForMarkers(
                offsets,
                ViewModel.Markers.Where(m => m.Type == SegmentType.Sprint).ToList(),
                (id, points) => new SprintSegment(id, points));
        }

        private void CreatePathsForMarkers(Offsets offsets, List<Segment> markers, Func<string, SKPoint[], MapObject> createMapObject)
        {
            var segmentsWithOffsets = markers
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point => point.ToMapCoordinate()).ToList()
                })
            .ToList();

            foreach (var segment in segmentsWithOffsets)
            {
                var segmentPath = SkiaPathFromSegment(offsets, segment.GameCoordinates);

                var path = createMapObject(segment.Segment.Id, segmentPath.Points);
                ZwiftMap.MapObjects.Add(path);
            }
        }

        private Offsets? CreatePathsForSegments(List<Segment> segments, World world)
        {
            if (!world.MapMostLeft.HasValue || !world.MapMostRight.HasValue)
            {
                throw new ArgumentException("Can't create paths if the bounding box for the world is missing");
            }

            var size = new Rect(
                new Point(
                    world.MapMostLeft.Value.X,
                    world.MapMostLeft.Value.Y),
                new Point(
                    world.MapMostRight.Value.X,
                    world.MapMostRight.Value.Y));

            if (!segments.Any())
            {
                return null;
            }

            var segmentsWithOffsets = segments
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point => point.ToMapCoordinate()).ToList()
                })
                .Select(x => new
                {
                    x.Segment,
                    x.GameCoordinates,
                    Offsets = new Offsets((float)size.Width, (float)size.Height, x.GameCoordinates, world.ZwiftId)
                })
                .ToList();

            var overallOffsets = Offsets
                .From(segmentsWithOffsets.Select(s => s.Offsets).ToList())
                .Translate((int)size.Left, (int)size.Top);

            foreach (var segment in segmentsWithOffsets)
            {
                var segmentPath = SkiaPathFromSegment(overallOffsets, segment.GameCoordinates);

                var path = new MapSegment(segment.Segment.Id, segmentPath.Points);
                ZwiftMap.MapObjects.Add(path);
            }

            var spawnPoints = world.SpawnPoints.Distinct(new SpawnPointComparer()).ToList();
            foreach (var spawnPoint in spawnPoints)
            {
                var segmentPath = ZwiftMap.MapObjects.OfType<MapSegment>()
                    .SingleOrDefault(mo => mo.SegmentId == spawnPoint.SegmentId);

                if (segmentPath != null)
                {
                    var spawnPointSegment = segments.Single(s => s.Id == spawnPoint.SegmentId);
                    var offset = 4;
                    var middle = spawnPointSegment.Points.Count / 2;

                    var startPoint = spawnPointSegment.Points[middle - offset];
                    var endPoint = spawnPointSegment.Points[middle + offset];

                    var middleBearing = TrackPoint.Bearing(startPoint, endPoint);

                    if (spawnPoint.Direction == SegmentDirection.BtoA)
                    {
                        // If the spawn point goes in the opposite direction
                        // of the segment then flip the bearing
                        middleBearing = (middleBearing + 180) % 360;
                    }

                    ZwiftMap.MapObjects.Add(new SpawnPointSegment(segmentPath.SegmentId, segmentPath.Points, middleBearing));
                }
            }

            return overallOffsets;
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<MapCoordinate> data)
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

        private void SetZwiftMap()
        {
            var stopwatch = Stopwatch.StartNew();

            var currentMap =
                ZwiftMap.MapObjects.SingleOrDefault(mo => mo is WorldMap);

            var worldId = ViewModel.Route.World?.Id;

            if (string.IsNullOrEmpty(worldId) && currentMap != null)
            {
                ZwiftMap.MapObjects.Clear();
            }

            if (!string.IsNullOrEmpty(worldId))
            {
                if (currentMap != null && !currentMap.Name.EndsWith($"-{worldId}"))
                {
                    Debug.WriteLine("Clearing objects from map because a different world was selected");
                    ZwiftMap.MapObjects.Clear();
                }

                if (currentMap == null)
                {
                    ZwiftMap.MapObjects.Add(new WorldMap(worldId));
                    AddPathsToMap(ViewModel.Route.World!);
                }
            }

            stopwatch.Stop();
            Debug.WriteLine($"SetZwiftMap(): {stopwatch.ElapsedMilliseconds}ms");
        }

        private void MainWindow_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= MainWindow_OnActivated;

            Dispatcher.UIThread.InvokeAsync(() => ViewModel.CheckForNewVersion());
            Dispatcher.UIThread.InvokeAsync(() => ViewModel.CheckLastOpenedVersion());
        }

        // Bunch of event handlers referenced by the XAML that
        // ReSharper doesn't detect here (generated code might
        // not yet exist)
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is SegmentSequenceViewModel viewModel)
            {
                ViewModel.HighlightSegment(viewModel.SegmentId);
            }
            else
            {
                ViewModel.ClearSegmentHighlight();
            }
        }

        private void MarkersOnRouteListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is MarkerViewModel viewModel)
            {
                ViewModel.HighlightMarker(viewModel.Id);
            }
            else
            {
                ViewModel.ClearMarkerHighlight();
            }
        }

        private void RouteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == ViewModel.Route.Last)
                {
                    ViewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.SelectedItem = RouteListView.Items.Cast<object>().Last();
                    }
                }
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZwiftMap.Zoom(ZwiftMap.ZoomLevel + 0.1f, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZwiftMap.Zoom(ZwiftMap.ZoomLevel - 0.1f, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZwiftMap.ZoomAll();
        }


        private void ZoomRoute_Click(object? sender, RoutedEventArgs e)
        {
            ZwiftMap.ZoomExtent("route");
        }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local

        private void ZwiftMap_OnMapObjectSelected(object? sender, MapObjectSelectedEventArgs e)
        {
            if (e.MapObject is MapSegment mapSegment)
            {
                var segment = ViewModel.Segments.SingleOrDefault(s => s.Id == mapSegment.SegmentId);

                if (segment != null)
                {
                    ViewModel.SelectSegmentCommand.Execute(segment);
                }
            }
        }
    }
}