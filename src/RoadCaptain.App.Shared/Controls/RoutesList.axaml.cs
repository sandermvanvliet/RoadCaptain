﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.Shared.Controls
{
    public partial class RoutesList : UserControl
    {
        public event EventHandler<RouteSelectedEventArgs>? RouteSelected;

        public static readonly StyledProperty<RouteViewModel?> SelectedRouteProperty =
            AvaloniaProperty.Register<RoutesList, RouteViewModel?>(nameof(SelectedRoute));

        public static readonly StyledProperty<RouteViewModel[]> RoutesProperty =
            AvaloniaProperty.Register<RoutesList, RouteViewModel[]>(nameof(Routes));

        public RoutesList()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RoutesListBox_OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (sender is not ListBox listBox)
            {
                return;
            }

            if (listBox.SelectedItem is not RouteViewModel selectedRoute)
            {
                return;
            }

            RouteSelected?.Invoke(this, new RouteSelectedEventArgs(selectedRoute, SelectionIntent.SelectAndChoose));
        }

        private void RoutesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
            {
                return;
            }

            if (listBox.SelectedItem is not RouteViewModel selectedRoute)
            {
                return;
            }

            RouteSelected?.Invoke(this, new RouteSelectedEventArgs(selectedRoute, SelectionIntent.Select));
        }

        public RouteViewModel? SelectedRoute
        {
            get => GetValue(SelectedRouteProperty);
            set => SetValue(SelectedRouteProperty, value);
        }

        public RouteViewModel[] Routes
        {
            get => GetValue(RoutesProperty);
            set => SetValue(RoutesProperty, value);
        }
    }

    public class RouteSelectedEventArgs : EventArgs
    {
        public RouteViewModel Route { get; }
        public SelectionIntent Intent { get; }

        public RouteSelectedEventArgs(RouteViewModel route, SelectionIntent intent)
        {
            Route = route;
            Intent = intent;
        }
    }

    public enum SelectionIntent
    {
        Unknown,
        Select,
        SelectAndChoose
    }
}