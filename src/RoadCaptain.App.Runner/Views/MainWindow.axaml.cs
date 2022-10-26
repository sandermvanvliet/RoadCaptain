// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Controls;
using RoadCaptain.Ports;
using SkiaSharp;

namespace RoadCaptain.App.Runner.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly ISegmentStore _segmentStore;
        private bool _isFirstTimeActivation = true;
        private CancellationTokenSource? _cancellationTokenSource;

        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IGameStateReceiver gameStateReceiver, ISegmentStore segmentStore)
        {
            _viewModel = viewModel;
            _segmentStore = segmentStore;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.RoutePath) && !string.IsNullOrEmpty(_viewModel.RoutePath))
                {
                    RebelRouteCombo.SelectedItem = null;
                }

                if (args.PropertyName == nameof(MainWindowViewModel.Route))
                {
                    ShowRouteOnMap(viewModel.Route);
                }
            };

            gameStateReceiver.ReceiveRoute(route => viewModel.Route = RouteModel.From(route, _segmentStore.LoadSegments(route.World, route.Sport), _segmentStore.LoadMarkers(route.World)));
            gameStateReceiver.ReceiveGameState(viewModel.UpdateGameState);

            DataContext = viewModel;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Selector_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox?.SelectedItem != null)
            {
                _viewModel.RoutePath = null;

                if (comboBox.SelectedItem is PlannedRoute selectedRoute)
                {
                    _viewModel.Route = RouteModel.From(selectedRoute, _segmentStore.LoadSegments(selectedRoute.World, selectedRoute.Sport), _segmentStore.LoadMarkers(selectedRoute.World));
                }
                else
                {
                    _viewModel.Route = new RouteModel();
                }
            }
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            if(_isFirstTimeActivation)
            {
                _isFirstTimeActivation = false;

                Dispatcher.UIThread.InvokeAsync(() => _viewModel.Initialize());
                Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckForNewVersion());
                Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckLastOpenedVersion());
            }
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void RebelRouteCombo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // This prevents the situation where the PointerPressed event bubbles
            // up to the window and initiates the window drag operation.
            // It fixes a bug where the combo box can't be opened.
            e.Handled = true;
        }

        private void ShowRouteOnMap(RouteModel route)
        {
            _cancellationTokenSource?.Cancel();

            using var updateScope = ZwiftMap.BeginUpdate();
            
            ZwiftMap.MapObjects.Clear();

            if (route.World == null)
            {
                return;
            }

            ZwiftMap.MapObjects.Add(new WorldMap(route.World.Id));

            var segmentsOnRoute = _segmentStore.LoadSegments(route.World, route.Sport);

            var mapSegments = CreatePathsForSegments(segmentsOnRoute, route.World);

            var routePoints = RoutePathPointsFrom(route.PlannedRoute.RouteSegmentSequence, mapSegments);

            var routePath = new RoutePath(routePoints) { IsVisible = true };
            ZwiftMap.MapObjects.Add(routePath);

            ZwiftMap.ZoomExtent("route");
            
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!(_cancellationTokenSource?.IsCancellationRequested ?? false))
                        {
                            routePath.MoveNext();
                            ZwiftMap.InvalidateVisual();

                            Thread.Sleep(40);
                        }
                    }
                    catch
                    {
                        // Nop
                    }
                },
                _cancellationTokenSource.Token);
        }
        
        private static SKPoint[] RoutePathPointsFrom(IEnumerable<SegmentSequence> routeSequence, List<MapSegment> mapSegments)
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
        
        private List<MapSegment> CreatePathsForSegments(List<Segment> segments, World world)
        {
            var segmentPaths = new List<MapSegment>();

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
                return segmentPaths;
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
                segmentPaths.Add(path);
            }

            return segmentPaths;
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
    }
}

