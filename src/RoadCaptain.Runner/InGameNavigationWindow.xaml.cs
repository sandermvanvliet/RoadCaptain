// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.Runner.ViewModels;
using Point = System.Drawing.Point;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for InGameNavigationWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private InGameNavigationWindowViewModel _viewModel;
        
        public InGameNavigationWindow(IGameStateReceiver gameStateReceiver, 
            MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            
            InitializeComponent();

            gameStateReceiver.Register(
                null,
                null,
                GameStateReceived);
        }

        private void Window_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void InGameNavigationWindow_OnInitialized(object sender, EventArgs e)
        {
            if (AppSettings.Default.InGameWindowLocation != Point.Empty)
            {
                Left = AppSettings.Default.InGameWindowLocation.X;
                Top = AppSettings.Default.InGameWindowLocation.Y;
            }
        }

        private void InGameNavigationWindow_OnLocationChanged(object sender, EventArgs e)
        {
            AppSettings.Default.InGameWindowLocation = new Point((int)Left, (int)Top);
            AppSettings.Default.Save();
        }

        private void InGameNavigationWindow_OnActivated(object sender, EventArgs e)
        {
            _viewModel = DataContext as InGameNavigationWindowViewModel;
        }

        private void GameStateReceived(GameState gameState)
        {
            try
            {
                _viewModel.UpdateGameState(gameState);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to update game state");
            }
        }
    }
}
