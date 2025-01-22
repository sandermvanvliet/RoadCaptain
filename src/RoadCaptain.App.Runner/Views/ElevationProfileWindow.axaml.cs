// Copyright (c) 2025 Sander van Vliet
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
    public partial class ElevationProfileWindow : Window
    {
        private ElevationProfileWindowViewModel? _viewModel;

        // ReSharper disable once UnusedMember.Global because this is only used by the Avalonia designer
        public ElevationProfileWindow()
        {
            InitializeComponent();
        }

        // ReSharper disable once UnusedMember.Global because this is called by the IoC container
        public ElevationProfileWindow(IGameStateReceiver gameStateReceiver, IUserPreferences userPreferences)
        {
            this.UseWindowStateTracking(
                userPreferences.ElevationProfileWindowLocation,
                newWindowLocation =>
                {
                    userPreferences.ElevationProfileWindowLocation = newWindowLocation;
                    userPreferences.Save();
                });

            InitializeComponent();

            gameStateReceiver.ReceiveGameState(GameStateReceived);
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;

            _viewModel = DataContext as ElevationProfileWindowViewModel ?? throw new Exception("");
            
            this.Bind(_viewModel.ToggleElevationProfileCommand).To(Key.E).WithPlatformModifier();
        }

        private void GameStateReceived(GameState gameState)
        {
            _viewModel?.UpdateGameState(gameState);
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

