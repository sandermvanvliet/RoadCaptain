using System;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.ViewModels;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCallingStartRouteCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private AppSettings _appSettings;
        private Configuration _configuration;

        public WhenCallingStartRouteCommand()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents());
            var windowService = new StubWindowService(null);

            _appSettings = new AppSettings();
            _configuration = new Configuration(null);

            StubRouteStore routeStore = new StubRouteStore();
            _viewModel = new MainWindowViewModel(
                _configuration,
                new AppSettings(),
                windowService,
                _gameStateDispatcher,
                new LoadRouteUseCase(_gameStateDispatcher, routeStore),
                routeStore,
                null);
        }


        [Fact]
        public void RoutePathIsStoredInAppSettings()
        {
            _viewModel.RoutePath = "someroute.json";

            StartRoute();

            _appSettings
                .Route
                .Should()
                .BeEquivalentTo(_viewModel.RoutePath);
        }


        [Fact]
        public void RoutePathIsStoredInConfiguration()
        {
            _viewModel.RoutePath = "someroute.json";

            StartRoute();

            _configuration
                .Route
                .Should()
                .BeEquivalentTo(_viewModel.RoutePath);
        }

        [Fact]
        public void RouteIsDispatched()
        {
            _viewModel.RoutePath = "someroute.json";

            StartRoute();

            GetDispatchedRoute()
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void WaitingForConnectionStateIsDispatched()
        {
            StartRoute();

            GetFirstDispatchedGameState()
                .Should()
                .BeOfType<WaitingForConnectionState>();
        }

        private void StartRoute()
        {
            _viewModel.StartRouteCommand.Execute(null);
        }

        private GameState GetFirstDispatchedGameState()
        {
            // This method is meant to collect the first game
            // state update that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first game state dispatch call without having
            // to do Thread.Sleep() calls.

            GameState lastState = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no game state is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            _gameStateDispatcher.Register(
                null,
                null,
                gameState =>
                {
                    lastState = gameState;

                    // Cancel after the first state is dispatched.
                    tokenSource.Cancel();
                });

            // This call blocks until the callback is invoked or
            // the cancellation token expires automatically.
            _gameStateDispatcher.Start(tokenSource.Token);

            return lastState;
        }

        private PlannedRoute GetDispatchedRoute()
        {
            // This method is meant to collect the first game
            // state update that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first game state dispatch call without having
            // to do Thread.Sleep() calls.

            PlannedRoute plannedRoute = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no game state is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            _gameStateDispatcher.Register(
                route =>
                {
                    plannedRoute = route;

                    // Cancel after the first state is dispatched.
                    tokenSource.Cancel();
                },
                null,
                null);

            // This call blocks until the callback is invoked or
            // the cancellation token expires automatically.
            _gameStateDispatcher.Start(tokenSource.Token);

            return plannedRoute;
        }
    }
}
