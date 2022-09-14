using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using Serilog;
using Point = System.Drawing.Point;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IUserPreferences _userPreferences;
        private InGameNavigationWindowViewModel? _viewModel;
        private bool _isFirstTimeActivation = true;

        // ReSharper disable once UnusedMember.Global this is only used for the Avalonia UI designer
        public InGameNavigationWindow()
        {
            _userPreferences = new DummyUserPreferences();
            _monitoringEvents = new MonitoringEventsWithSerilog(new LoggerConfiguration().WriteTo.Debug().CreateLogger());

            InitializeComponent();
            
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        public InGameNavigationWindow(IGameStateReceiver gameStateReceiver, 
            MonitoringEvents monitoringEvents, IUserPreferences userPreferences)
        {
            _monitoringEvents = monitoringEvents;
            _userPreferences = userPreferences;

            InitializeComponent();
            
#if DEBUG
            this.AttachDevTools();
#endif

            gameStateReceiver.ReceiveGameState(GameStateReceived);
            gameStateReceiver.ReceiveLastSequenceNumber(sequenceNumber => _viewModel.LastSequenceNumber = sequenceNumber);
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
            if (_isFirstTimeActivation)
            {
                _isFirstTimeActivation = false;

                _viewModel = DataContext as InGameNavigationWindowViewModel ?? throw new Exception("");

                var modifier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? KeyModifiers.Meta
                    : KeyModifiers.Control;

                KeyBindings.Add(new KeyBinding
                    { Command = _viewModel.EndActivityCommand, Gesture = new KeyGesture(Key.X, modifier) });
            }
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
