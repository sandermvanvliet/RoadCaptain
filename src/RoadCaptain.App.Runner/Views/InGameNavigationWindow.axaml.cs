// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using Serilog;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private InGameNavigationWindowViewModel? _viewModel;

        // ReSharper disable once UnusedMember.Global this is only used for the Avalonia UI designer
        public InGameNavigationWindow()
        {
            _monitoringEvents = new MonitoringEventsWithSerilog(new LoggerConfiguration().WriteTo.Debug().CreateLogger());

            InitializeComponent();
        }

        public InGameNavigationWindow(IGameStateReceiver gameStateReceiver,
            MonitoringEvents monitoringEvents, IUserPreferences userPreferences)
        {
            this.UseWindowStateTracking(
                userPreferences.InGameWindowLocation,
                newWindowLocation =>
                {
                    userPreferences.InGameWindowLocation = newWindowLocation;
                    userPreferences.Save();
                });

            _monitoringEvents = monitoringEvents;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            gameStateReceiver.ReceiveGameState(GameStateReceived);
            gameStateReceiver.ReceiveLastSequenceNumber(sequenceNumber => _viewModel.LastSequenceNumber = sequenceNumber);
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= InGameNavigationWindow_OnActivated;
            
            _viewModel = DataContext as InGameNavigationWindowViewModel ?? throw new Exception("");

            var modifier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? KeyModifiers.Meta
                : KeyModifiers.Control;

            KeyBindings.Add(new KeyBinding { Command = _viewModel.EndActivityCommand, Gesture = new KeyGesture(Key.X, modifier) });
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

