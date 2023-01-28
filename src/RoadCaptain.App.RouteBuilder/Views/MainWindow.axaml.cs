// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Controls;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;
using ListBox = Avalonia.Controls.ListBox;
using Point = Avalonia.Point;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MainWindow : Window
    {
        private readonly MapObjectsSource _mapObjectsSource;
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
            ZwiftMap.LogDiagnostics = false;
            
            this.Bind(ViewModel.OpenRouteCommand).To(Key.O).WithPlatformModifier();
            this.Bind(ViewModel.SaveRouteCommand).To(Key.S).WithPlatformModifier();
            this.Bind(ViewModel.ClearRouteCommand).To(Key.R).WithPlatformModifier();
            this.Bind(ViewModel.RemoveLastSegmentCommand).To(Key.Z).WithPlatformModifier();

            _mapObjectsSource = new MapObjectsSource(ZwiftMap);
        }

        private MainWindowViewModel ViewModel { get; }

        private void WindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
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