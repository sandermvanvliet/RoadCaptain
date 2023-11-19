using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.Shared.Controls
{
    public partial class RoutesList : UserControl
    {
        public event EventHandler<RouteSelectedEventArgs> RouteSelected;
        public static readonly DirectProperty<RoutesList, RouteViewModel?> SelectedRouteProperty = AvaloniaProperty.RegisterDirect<RoutesList, RouteViewModel?>(
            nameof(SelectedRoute),
            control => control.SelectedRoute, 
            (control, value) => control.SelectedRoute = value);
        
        public static readonly DirectProperty<RoutesList, RouteViewModel[]> RoutesProperty = AvaloniaProperty.RegisterDirect<RoutesList, RouteViewModel[]>(
            nameof(Routes),
            control => control.Routes, 
            (control, value) => control.Routes = value);

        private RouteViewModel? _selectedRoute;
        private RouteViewModel[] _routes = Array.Empty<RouteViewModel>();

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

            SelectedRoute = selectedRoute;
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

            SelectedRoute = selectedRoute;
            RouteSelected?.Invoke(this, new RouteSelectedEventArgs(selectedRoute, SelectionIntent.Select));
        }

        public RouteViewModel? SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                if (value == null)
                {
                    this.Find<ListBox>("RoutesListBox").SelectedItem = null;
                }
            }
        }

        public RouteViewModel[] Routes
        {
            get => _routes;
            set => SetAndRaise(RoutesProperty, ref _routes, value);
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