﻿using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.Shared.Controls
{
    public partial class RoutesList : UserControl
    {
        public event EventHandler<RouteSelectedEventArgs> RouteSelected;

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

            RouteSelected.Invoke(this, new RouteSelectedEventArgs(selectedRoute, SelectionIntent.SelectAndChoose));
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

            RouteSelected.Invoke(this, new RouteSelectedEventArgs(selectedRoute, SelectionIntent.Select));
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