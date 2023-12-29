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
    public class WhenUpdatingMainWindowViewModel
    {
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly MainWindowViewModel _viewModel;

        public WhenUpdatingMainWindowViewModel()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents(), new PlatformPaths());
            var routeStore = new StubRouteStore();
            _viewModel = new MainWindowViewModel(new Configuration(null), 
                new DummyUserPreferences(),
                new StubWindowService(),
                _gameStateDispatcher,
                routeStore,
                new StubVersionChecker(), 
                new SegmentStore(new Shared.NopMonitoringEvents()),
                new NoZwiftCredentialCache(),
                new NopMonitoringEvents(),
                new DummyApplicationFeatures(),
                new Zwift(null!),
                new PlatformPaths());
        }

        [Fact]
        public void GivenZwiftTokenIsSetAndGameStateIsInvalidCredentials_LoggedInToZwiftIsFalse()
        {
            GivenLoggedInUser();

            _viewModel.UpdateGameState(new InvalidCredentialsState(new Exception("BANG")));

            _viewModel
                .LoggedInToZwift
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GivenZwiftTokenIsSetAndGameStateIsInvalidCredentials_ZwiftAccessTokenIsNull()
        {
            GivenLoggedInUser();

            _viewModel.UpdateGameState(new InvalidCredentialsState(new Exception("BANG")));

            _viewModel
                .ZwiftAccessToken
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenZwiftTokenIsSetAndGameStateIsInvalidCredentials_ZwiftAvatarUriIsNull()
        {
            GivenLoggedInUser();

            _viewModel.UpdateGameState(new InvalidCredentialsState(new Exception("BANG")));

            _viewModel
                .ZwiftAvatarUri
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenZwiftTokenIsSetAndGameStateIsInvalidCredentials_ZwiftNameIsNull()
        {
            GivenLoggedInUser();

            _viewModel.UpdateGameState(new InvalidCredentialsState(new Exception("BANG")));

            _viewModel
                .ZwiftName
                .Should()
                .BeNull();
        }

        private void GivenLoggedInUser()
        {
            _viewModel.ZwiftAccessToken = "some token";
            _viewModel.ZwiftName = "name";
            _viewModel.LoggedInToZwift = true;
            _viewModel.ZwiftAvatarUri = "avatar";
        }
        
        private GameState? GetFirstDispatchedGameState()
        {
            // This method is meant to collect the first game
            // state update that is sent through the dispatcher.
            // By using the cancellation token in the callback
            // we can ensure that we can block while waiting for
            // that first game state dispatch call without having
            // to do Thread.Sleep() calls.

            GameState? lastState = null;

            // Use a cancellation token with a time-out so that
            // the test fails if no game state is dispatched.
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            _gameStateDispatcher.ReceiveGameState(
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
    }
}
