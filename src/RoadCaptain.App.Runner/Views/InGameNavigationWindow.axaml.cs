// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using Serilog;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IUserPreferences _userPreferences;
        private InGameNavigationWindowViewModel? _viewModel;

        // ReSharper disable once UnusedMember.Global this is only used for the Avalonia UI designer
        public InGameNavigationWindow()
        {
            _monitoringEvents = new MonitoringEventsWithSerilog(new LoggerConfiguration().WriteTo.Debug().CreateLogger());
            _userPreferences = new DummyUserPreferences();

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
            _userPreferences = userPreferences;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            gameStateReceiver.ReceiveGameState(GameStateReceived);
            gameStateReceiver.ReceiveLastSequenceNumber(sequenceNumber => _viewModel!.LastSequenceNumber = sequenceNumber);
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= InGameNavigationWindow_OnActivated;
            
            _viewModel = DataContext as InGameNavigationWindowViewModel ?? throw new Exception("");

            if (_userPreferences.ShowElevationPlotInGame)
            {
                _viewModel.ToggleElevationPlotCommand.Execute(_userPreferences.ShowElevationPlotInGame);
            }
            
            this.Bind(_viewModel.EndActivityCommand).To(Key.X).WithPlatformModifier();
            this.Bind(_viewModel.ToggleElevationPlotCommand).To(Key.E).WithPlatformModifier();
        }

        private void GameStateReceived(GameState gameState)
        {
            try
            {
                _viewModel!.UpdateGameState(gameState);
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

