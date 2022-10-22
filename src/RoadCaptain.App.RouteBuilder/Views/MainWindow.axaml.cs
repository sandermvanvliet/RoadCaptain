// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
                    ViewModel.ClearSegmentHighlight();
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
                    SetZwiftMap();

                    SynchronizeRouteSegmentsOnZwiftMap();

                    break;
                case nameof(ViewModel.RiderPosition):
                    var routePath = ZwiftMap.MapObjects.OfType<RoutePath>().SingleOrDefault();
                    
                    if (ViewModel.RiderPosition == null && routePath != null)
                    {
                        routePath.Reset();
                    }

                    routePath?.MoveNext();

                    ZwiftMap.InvalidateVisual();
                    
                    break;
            }
        }

        private void SynchronizeRouteSegmentsOnZwiftMap()
        {
            var mapSegments = ZwiftMap
                .MapObjects
                .OfType<MapSegment>()
                .ToList();

            var routeHasSegments = ViewModel.Route.Sequence.Any();
            
            // Toggle the spawn point flag
            foreach (var segment in mapSegments)
            {
                segment.IsSpawnPoint = !routeHasSegments && ViewModel.Route.World.SpawnPoints.Any(s => s.SegmentId == segment.SegmentId);
            }
            
            // Synchronize the route path with the route
            var routePath = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is RoutePath);

            var routePathPoints = RoutePathPointsFrom(ViewModel.Route.Sequence, mapSegments);

            if (routePath != null)
            {
                ZwiftMap.MapObjects.Remove(routePath);
            }

            routePath = new RoutePath(routePathPoints);
            ZwiftMap.MapObjects.Add(routePath);
            
            // Force re-render
            ZwiftMap.InvalidateVisual();
        }

        private SKPoint[] RoutePathPointsFrom(IEnumerable<SegmentSequenceViewModel> routeSequence, List<MapSegment> mapSegments)
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
            ZwiftMap.InvalidateVisual();
        }

        private void AddPathsToMap()
        {
            if (!ViewModel.Segments.Any())
            {
                return;
            }

            var map = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is WorldMap);

            if (map == null || ViewModel.Route.World == null)
            {
                return;
            }

            CreatePathsForSegments(ViewModel.Segments, ViewModel.Route.World);
        }

        private void CreatePathsForSegments(List<Segment> segments, World world)
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

            var segmentPaths = new Dictionary<string, SKPath>();

            if (!segments.Any())
            {
                return;
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
                var skiaPathFromSegment = SkiaPathFromSegment(overallOffsets, segment.GameCoordinates);

                segmentPaths.Add(segment.Segment.Id, skiaPathFromSegment);
            }

            foreach (var segmentPath in segmentPaths)
            {
                var path = new MapSegment(segmentPath.Key, segmentPath.Value.Points,
                    world.SpawnPoints.Any(s => s.SegmentId == segmentPath.Key));
                ZwiftMap.MapObjects.Add(path);
            }
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
            var currentMap =
                ZwiftMap.MapObjects.SingleOrDefault(mo => mo is WorldMap && mo.Name.StartsWith("worldMap-"));

            var worldId = ViewModel.Route.World?.Id;

            if (string.IsNullOrEmpty(worldId) && currentMap != null)
            {
                ZwiftMap.MapObjects.Clear();
            }

            if (!string.IsNullOrEmpty(worldId))
            {
                var newMap = new WorldMap(worldId);

                if (currentMap != null && !currentMap.Name.EndsWith($"-{worldId}"))
                {
                    ZwiftMap.MapObjects.Clear();

                    // Insert because we want it to be at the lowest level
                    ZwiftMap.MapObjects.Insert(0, newMap);

                    AddPathsToMap();
                }
                else if (currentMap == null)
                {
                    // Insert because we want it to be at the lowest level
                    ZwiftMap.MapObjects.Insert(0, newMap);
                    AddPathsToMap();
                }
            }
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
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local

        private void ZwiftMap_OnMapObjectSelected(object? sender, MapObjectSelectedEventArgs e)
        {
            Debug.WriteLine($"Selected map object: {e.MapObject.GetType().Name} {e.MapObject.Name}");

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