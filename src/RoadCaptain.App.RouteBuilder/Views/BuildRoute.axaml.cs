// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;
using InvalidOperationException = System.InvalidOperationException;
using Point = Avalonia.Point;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class BuildRoute : UserControl
    {
        private readonly MapObjectsSource _mapObjectsSource;
        private BuildRouteViewModel _viewModel = default!;

        public BuildRoute()
        {
            
            InitializeComponent();
            
            ZwiftMap.RenderPriority = new ZwiftMapRenderPriority();
            ZwiftMap.LogDiagnostics = false;

            _mapObjectsSource = new MapObjectsSource(ZwiftMap);
        }

        private void BuildRouteViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.SelectedSegment):
                    // Reset any manually selected item in the list
                    using (ZwiftMap.BeginUpdate())
                    {
                        _viewModel.ClearSegmentHighlight();

                        _mapObjectsSource.SynchronizeRouteSegmentsOnZwiftMap(_viewModel.Route);
                    }
                    break;
                case nameof(_viewModel.HighlightedSegment):
                    _mapObjectsSource.HighlightOnZwiftMap(_viewModel.HighlightedSegment);
                    break;
                case nameof(_viewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteSegmentListView.RouteListView.ItemCount > 0)
                    {
                        RouteSegmentListView.RouteListView.ScrollIntoView(RouteSegmentListView.RouteListView.ItemCount - 1);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    using (ZwiftMap.BeginUpdate())
                    {
                        if (_viewModel.Route.ReadyToBuild && _viewModel.Segments.Any()) // Excluding markers here because they may not be available in Beta worlds
                        {
                            _mapObjectsSource.SetZwiftMap(_viewModel.Route, _viewModel.Segments, _viewModel.Markers);
                            _mapObjectsSource.SynchronizeRouteSegmentsOnZwiftMap(_viewModel.Route);
                        }
                        else
                        {
                            _mapObjectsSource.Clear();
                        }
                    }

                    break;
                case nameof(_viewModel.RiderPosition):
                    var routePath = ZwiftMap.MapObjects.OfType<RoutePath>().SingleOrDefault();

                    if (routePath != null)
                    {
                        if (_viewModel.RiderPosition == null)
                        {
                            routePath.Reset();
                            routePath.ShowFullPath = false;
                        }
                        else
                        {
                            routePath.ShowFullPath = true;
                            routePath.MoveNext();
                        }
                    }

                    InvalidateZwiftMap();

                    break;
                case nameof(_viewModel.ShowClimbs):
                    _mapObjectsSource.ToggleClimbs(_viewModel.ShowClimbs);
                    break;
                case nameof(_viewModel.ShowSprints):
                    _mapObjectsSource.ToggleSprints(_viewModel.ShowSprints);
                    break;
            }
        }

        private void InvalidateZwiftMap([CallerMemberName] string? caller = null)
        {
            Debug.WriteLine($"[InvalidateZwiftMap] {caller}");
            ZwiftMap.InvalidateVisual();
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

        private void ZwiftMap_OnMapObjectSelected(object? sender, MapObjectSelectedEventArgs e)
        {
            if (e.MapObject is MapSegment mapSegment)
            {
                var segment = _viewModel.Segments.SingleOrDefault(s => s.Id == mapSegment.SegmentId);

                if (segment != null)
                {
                    _viewModel.SelectSegmentCommand.Execute(segment);
                }
            }
            if (e.MapObject is SpawnPointSegment spawnPointSegment)
            {
                var segment = _viewModel.Segments.SingleOrDefault(s => s.Id == spawnPointSegment.SegmentId);

                if (segment != null)
                {
                    _viewModel.SelectSegmentCommand.Execute(segment);
                }
            }
        }

        private void StyledElement_OnInitialized(object? sender, EventArgs e)
        {
            if (DataContext == null)
            {
                throw new InvalidOperationException("Can't initialize when the DataContext is null");
            }
            
            _viewModel = (BuildRouteViewModel)DataContext;
            _viewModel.PropertyChanged += BuildRouteViewModelPropertyChanged;
        }
    }
}
