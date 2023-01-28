// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RoadCaptain.Ports;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => _viewModel.CheckForNewVersion());
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox.SelectedItem != null)
            {
                _viewModel.RoutePath = null;
                _viewModel.Route = comboBox.SelectedItem as PlannedRoute;
            }
        }
    }
}
