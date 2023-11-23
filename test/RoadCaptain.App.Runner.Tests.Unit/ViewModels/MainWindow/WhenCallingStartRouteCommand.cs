// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCallingStartRouteCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly DummyUserPreferences _userPreferences;
        private readonly Configuration _configuration;
        private readonly StubWindowService _windowService;
        private readonly PlannedRoute _plannedRoute;

        public WhenCallingStartRouteCommand()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents(), new PlatformPaths());
            _gameStateDispatcher.LoggedIn();
            
            _windowService = new StubWindowService();

            _userPreferences = new DummyUserPreferences();
            _configuration = new Configuration(null);

            StubRouteStore routeStore = new();
            _plannedRoute = routeStore.LoadFrom("someroute.json");

            _viewModel = new MainWindowViewModel(
                _configuration,
                _userPreferences,
                _windowService,
                _gameStateDispatcher,
                routeStore,
                new StubVersionChecker(), 
                new SegmentStore(),
                new NoZwiftCredentialCache(),
                new NopMonitoringEvents(),
                new DummyApplicationFeatures(),
                new Zwift(null!));
        }


        [Fact]
        public void RoutePathIsStoredInUserPreferences()
        {
            _windowService.ShowSelectRouteDialogResult = new RouteModel { Uri = new Uri("file:///c:/temp/someroute.json"), PlannedRoute = _plannedRoute };
            _viewModel.SearchRouteCommand.Execute(null);

            StartRoute();

            _userPreferences
                .Route
                .Should()
                .BeEquivalentTo(_viewModel.RoutePath);
        }


        [Fact]
        public void RoutePathIsStoredInConfiguration()
        {
            _windowService.ShowSelectRouteDialogResult = new RouteModel { Uri = new Uri("file:///c:/temp/someroute.json"), PlannedRoute = _plannedRoute };
            _viewModel.SearchRouteCommand.Execute(null);

            StartRoute();

            _configuration
                .Route
                .Should()
                .BeEquivalentTo(_viewModel.RoutePath);
        }

        [Fact]
        public void StartRouteIsDispatched()
        {
            _windowService.ShowSelectRouteDialogResult = new RouteModel { Uri = new Uri("file:///c:/temp/someroute.json"), PlannedRoute = _plannedRoute };
            _viewModel.SearchRouteCommand.Execute(null);

            StartRoute();

            GetLastDispatchedGameState()
                .Should()
                .BeOfType<ReadyToGoState>();
        }

        private void StartRoute()
        {
            _viewModel.StartRouteCommand.Execute(null);
        }

        private GameState? GetLastDispatchedGameState()
        {
            // This method is meant to collect the first game
            // state update that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first game state dispatch call without having
            // to do Thread.Sleep() calls.

            GameState? result = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no game state is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            
            _gameStateDispatcher.ReceiveGameState(
                gameState =>
                {
                    result = gameState;

                    if (result is ReadyToGoState)
                    {
                        tokenSource.Cancel();
                    }
                });

            // This call blocks until the callback is invoked or
            // the cancellation token expires automatically.
            _gameStateDispatcher.Start(tokenSource.Token);

            return result;
        }
    }
}

