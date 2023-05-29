using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RoadCaptain.App.Runner.ViewModels;

namespace RoadCaptain.App.Runner.Views
{
    public partial class SelectRouteWindow : Window
    {
        private SelectRouteWindowViewModel _viewModel;
        
        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public SelectRouteWindow()
#pragma warning restore CS8618
        {
            InitializeComponent();
        }
        

        public SelectRouteWindow(SelectRouteWindowViewModel viewModel)
        {
            _viewModel = viewModel;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SelectRouteWindowViewModel.SelectedRoute))
                {
                    SelectedRoute = _viewModel.SelectedRoute?.AsRouteModel();
                }
            };
            
            DataContext = viewModel;
            
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public RouteModel? SelectedRoute { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void RoutesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void RepositoryComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox)
            {
                return;
            }

            if (comboBox.SelectedItem is string selectedValue)
            {
                await _viewModel.LoadRoutesForRepositoryAsync(selectedValue);
            }
        }
    }
}