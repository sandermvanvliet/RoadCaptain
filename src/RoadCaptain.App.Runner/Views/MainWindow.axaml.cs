using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        
        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IGameStateReceiver gameStateReceiver)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.RoutePath) && !string.IsNullOrEmpty(_viewModel.RoutePath))
                {
                    RebelRouteCombo.SelectedItem = null;
                }
            };

            gameStateReceiver.Register(route => viewModel.Route = route, null, viewModel.UpdateGameState);

            DataContext = viewModel;

            InitializeComponent();
        }

        private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            // TODO: Make drag-move work
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
                _viewModel.Route = comboBox.SelectedItem as PlannedRoute;
            }
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            Task.Factory.StartNew(() => _viewModel.CheckForNewVersion());
        }
    }
}