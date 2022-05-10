using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using Point = System.Drawing.Point;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IUserPreferences _userPreferences;
        private InGameNavigationWindowViewModel _viewModel;

        public InGameNavigationWindow()
        {
            InitializeComponent();
        }
        
        public InGameNavigationWindow(IGameStateReceiver gameStateReceiver, 
            MonitoringEvents monitoringEvents, IUserPreferences userPreferences)
        {
            _monitoringEvents = monitoringEvents;
            _userPreferences = userPreferences;

            InitializeComponent();

            gameStateReceiver.Register(
                null,
                null,
                GameStateReceived);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (_userPreferences.InGameWindowLocation != null &&
                _userPreferences.InGameWindowLocation != new Point(0, 0))
            {
                Position = new PixelPoint(
                    _userPreferences.InGameWindowLocation.Value.X,
                    _userPreferences.InGameWindowLocation.Value.Y);
            }
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
            _viewModel = DataContext as InGameNavigationWindowViewModel;
        }

        private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e)
        {
            _userPreferences.InGameWindowLocation = new Point(Position.X, Position.Y);
            _userPreferences.Save();
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
