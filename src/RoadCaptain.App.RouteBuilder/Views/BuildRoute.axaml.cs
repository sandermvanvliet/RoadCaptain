using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;
using Point = Avalonia.Point;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class BuildRoute : UserControl
    {
        private readonly MapObjectsSource _mapObjectsSource;
        
        public BuildRoute()
        {
            ViewModel = (DataContext as BuildRouteViewModel)!; // Suppressed because it's initialized from XAML
            
            InitializeComponent();
            
            ZwiftMap.RenderPriority = new ZwiftMapRenderPriority();
            ZwiftMap.LogDiagnostics = false;

            _mapObjectsSource = new MapObjectsSource(ZwiftMap);
            
            //ViewModel.PropertyChanged += BuildRouteViewModelPropertyChanged;
        }

        private BuildRouteViewModel ViewModel { get; }

        private void BuildRouteViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedSegment):
                    // Reset any manually selected item in the list
                    using (ZwiftMap.BeginUpdate())
                    {
                        ViewModel.ClearSegmentHighlight();

                        _mapObjectsSource.SynchronizeRouteSegmentsOnZwiftMap(ViewModel.Route);
                    }
                    break;
                case nameof(ViewModel.HighlightedSegment):
                    _mapObjectsSource.HighlightOnZwiftMap(ViewModel.HighlightedSegment);
                    break;
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteSegmentListView.RouteListView.ItemCount > 0)
                    {
                        RouteSegmentListView.RouteListView.ScrollIntoView(RouteSegmentListView.RouteListView.ItemCount - 1);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    using (ZwiftMap.BeginUpdate())
                    {
                        _mapObjectsSource.SetZwiftMap(ViewModel.Route, ViewModel.Segments, ViewModel.Markers);
                        _mapObjectsSource.SynchronizeRouteSegmentsOnZwiftMap(ViewModel.Route);
                    }

                    break;
                case nameof(ViewModel.RiderPosition):
                    var routePath = ZwiftMap.MapObjects.OfType<RoutePath>().SingleOrDefault();

                    if (routePath != null)
                    {
                        if (ViewModel.RiderPosition == null)
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
                case nameof(ViewModel.ShowClimbs):
                    _mapObjectsSource.ToggleClimbs(ViewModel.ShowClimbs);
                    break;
                case nameof(ViewModel.ShowSprints):
                    _mapObjectsSource.ToggleSprints(ViewModel.ShowSprints);
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
                var segment = ViewModel.Segments.SingleOrDefault(s => s.Id == mapSegment.SegmentId);

                if (segment != null)
                {
                    ViewModel.SelectSegmentCommand.Execute(segment);
                }
            }
            if (e.MapObject is SpawnPointSegment spawnPointSegment)
            {
                var segment = ViewModel.Segments.SingleOrDefault(s => s.Id == spawnPointSegment.SegmentId);

                if (segment != null)
                {
                    ViewModel.SelectSegmentCommand.Execute(segment);
                }
            }
        }
    }
}