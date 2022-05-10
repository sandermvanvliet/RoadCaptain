using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private InGameNavigationWindowViewModel _viewModel;

        public InGameNavigationWindow()
        {
            InitializeComponent();
        }
        
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
            _viewModel = DataContext as InGameNavigationWindowViewModel;
        }

        private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e)
        {
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

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
    }
}
