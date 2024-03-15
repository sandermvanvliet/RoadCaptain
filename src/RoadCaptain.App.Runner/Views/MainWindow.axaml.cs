// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Skia;
using Avalonia.Threading;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Controls;
using RoadCaptain.Ports;
using Serilog.Core;
using SkiaSharp;
using Point = Avalonia.Point;
using Timer = System.Timers.Timer;

namespace RoadCaptain.App.Runner.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly ISegmentStore _segmentStore;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly Timer _animationTimer;
        private SKRect _elementBoundsMappedToViewport = SKRect.Empty;

        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            _monitoringEvents = new MonitoringEventsWithSerilog(Logger.None);

            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IGameStateReceiver gameStateReceiver, ISegmentStore segmentStore, MonitoringEvents monitoringEvents)
        {
            _viewModel = viewModel;
            _segmentStore = segmentStore;
            _monitoringEvents = monitoringEvents;
            _viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.Route))
                {
                    ShowRouteOnMap(viewModel.Route);
                }
            };

            gameStateReceiver.ReceiveRoute(route =>
            {
                if(route.World != null)
                {
                    viewModel.Route = Models.RouteModel.From(
                        route,
                        _segmentStore.LoadSegments(route.World, route.Sport),
                        _segmentStore.LoadMarkers(route.World));
                }
            });
            gameStateReceiver.ReceiveGameState(viewModel.UpdateGameState);

            _animationTimer = new Timer(100);
            _animationTimer.Elapsed += (_, _) => AnimateRouteOnTimerTick();

            DataContext = viewModel;

            InitializeComponent(true);

            ZwiftMap.LogDiagnostics = false;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;

            Dispatcher.UIThread.InvokeAsync(() => _viewModel.Initialize());
            Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckForNewVersion());
            Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckLastOpenedVersion());
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void ShowRouteOnMap(Models.RouteModel? route)
        {
            _animationTimer.Stop();
            
            using (ZwiftMap.BeginUpdate())
            {
                ZwiftMap.MapObjects.Clear();

                if (route?.World?.Id == null || route.PlannedRoute == null)
                {
                    return;
                }

                ZwiftMap.MapObjects.Add(new WorldMap(route.World.Id));

                var segmentsOnRoute = _segmentStore.LoadSegments(route.World, route.Sport);

                var mapSegments = CreatePathsForSegments(segmentsOnRoute, route.World);

                var routePoints = RoutePathPointsFrom(route.PlannedRoute.RouteSegmentSequence, mapSegments);

                var routePath = new RoutePath(routePoints) { IsVisible = true, ShowFullPath = true };
                ZwiftMap.MapObjects.Add(routePath);

                (_, _elementBoundsMappedToViewport) = ZwiftMap.ZoomExtent("route");
            }

            _animationTimer.Start();
        }

        private void AnimateRouteOnTimerTick()
        {
            const float minScale = 0.45f;

            var routePath = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is RoutePath) as RoutePath;
            if (routePath == null)
            {
                _animationTimer.Stop();
                return;
            }

            if (routePath.Current.HasValue &&
                (_elementBoundsMappedToViewport == SKRect.Empty ||
                 !CalculateMatrix.IsEntirelyWithin(_elementBoundsMappedToViewport, ZwiftMap.Bounds.ToSKRect())))
            {
                var currentPosition = routePath.Current;
                var currentOnViewport = ZwiftMap.MapToViewport(currentPosition.Value);
                (_, _elementBoundsMappedToViewport) = ZwiftMap.Zoom(minScale, currentOnViewport);
            }

            routePath.MoveNext();

            try
            {
                ZwiftMap.InvalidateVisual();
            }
            catch (ArgumentException e)
            {
                _monitoringEvents.Error(e, "Failed to invalidate map");
            }
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

