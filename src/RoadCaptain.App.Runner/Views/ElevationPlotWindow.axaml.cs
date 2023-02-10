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

namespace RoadCaptain.App.Runner.Views
{
    public partial class ElevationPlotWindow : Window
    {
        private ElevationPlotWindowViewModel _viewModel;

        public ElevationPlotWindow()
        {
            InitializeComponent();
        }

        public ElevationPlotWindow(IGameStateReceiver gameStateReceiver, IUserPreferences userPreferences)
        {
            this.UseWindowStateTracking(
                userPreferences.ElevationPlotWindowLocation,
                newWindowLocation =>
                {
                    userPreferences.ElevationPlotWindowLocation = newWindowLocation;
                    userPreferences.Save();
                });

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            gameStateReceiver.ReceiveGameState(GameStateReceived);
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;

            _viewModel = DataContext as ElevationPlotWindowViewModel ?? throw new Exception("");
            
            this.Bind(_viewModel.ToggleElevationPlotCommand).To(Key.E).WithPlatformModifier();
        }

        private void GameStateReceived(GameState gameState)
        {
            _viewModel.UpdateGameState(gameState);
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

