// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.ComponentModel;
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
            
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.OpenRouteCommand, Gesture = new KeyGesture(Key.O, modifier)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.SaveRouteCommand, Gesture = new KeyGesture(Key.S, modifier)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.ClearRouteCommand, Gesture = new KeyGesture(Key.R, modifier)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.RemoveLastSegmentCommand, Gesture = new KeyGesture(Key.Z, modifier)});

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
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    SetZwiftMap();

                    break;
                case nameof(ViewModel.Route.World) when ViewModel.Route.World != null:
                    SetZwiftMap();
                    break;
            }
        }

        private void SetZwiftMap()
        {
            var currentMap = ZwiftMap.MapObjects.SingleOrDefault(mo => mo is WorldMap && mo.Name.StartsWith("worldMap-"));

            var worldId = ViewModel.Route.World?.Id;

            if (string.IsNullOrEmpty(worldId) && currentMap != null)
            {
                ZwiftMap.MapObjects.Remove(currentMap);
            }
            
            if(!string.IsNullOrEmpty(worldId))
            {
                var newMap = new WorldMap(worldId);

                if (currentMap != null && !currentMap.Name.EndsWith($"-{worldId}"))
                {
                    ZwiftMap.MapObjects.Remove(currentMap);

                    // Insert because we want it to be at the lowest level
                    ZwiftMap.MapObjects.Insert(0, newMap);
                }
                else if(currentMap == null)
                {
                    // Insert because we want it to be at the lowest level
                    ZwiftMap.MapObjects.Insert(0, newMap);
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
            //SkElement.ZoomIn(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));
            ZwiftMap.Zoom(ZwiftMap.ZoomLevel+0.1f, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }
        
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            //SkElement.ZoomOut(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));
            ZwiftMap.Zoom(ZwiftMap.ZoomLevel-0.1f, new Point(Bounds.Width / 2, Bounds.Height / 2));
        }
        
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            //SkElement.ResetZoom();
            ZwiftMap.ZoomAll();
        }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local

        private void ZwiftMap_OnMapObjectSelected(object? sender, MapObjectSelectedEventArgs e)
        {

        }
    }
}
