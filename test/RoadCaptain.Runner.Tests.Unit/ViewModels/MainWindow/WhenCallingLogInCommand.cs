using System;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;
using RoadCaptain.UseCases;
using Xunit;
using TokenResponse = RoadCaptain.Runner.Models.TokenResponse;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCallingLogInCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly StubWindowService _windowService;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;

        public WhenCallingLogInCommand()
        {
            _windowService = new StubWindowService(null);
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents());

            StubRouteStore routeStore = new StubRouteStore();
            _viewModel = new MainWindowViewModel(new Configuration(null),
                new AppSettings(),
                _windowService,
                _gameStateDispatcher,
                new LoadRouteUseCase(_gameStateDispatcher, routeStore),
                routeStore);

            _windowService
                .Overrides
                .Add(typeof(ZwiftLoginWindow), new ZwiftLoginWindow());
        }

        [StaFact]
        public void LogInDialogIsOpened()
        {
            LogIn();

            _windowService
                .LogInDialogInvocations
                .Should()
                .Be(1);
        }

        [StaFact]
        public void GivenLogInDialogIsCanceled_ZwiftAccessTokenRetainsOriginalValue()
        {
            _windowService.LogInDialogResult = null;

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .BeNull();
        }

        [StaFact]
        public void GivenUserLogsInButTokenIsEmpty_ZwiftAccessTokenRetainsOriginalValue()
        {
            _windowService.LogInDialogResult = new TokenResponse();

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .BeNull();
        }

        [StaFact]
        public void GivenUserLoggedIn_ZwiftAccessTokenIsSet()
        {
            _windowService.LogInDialogResult = new TokenResponse { AccessToken = "some token" };

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .Be("some token");
        }

        [StaFact]
        public void GivenUserLoggedIn_ZwiftNameIsSet()
        {
            _windowService.LogInDialogResult = new TokenResponse
            {
                AccessToken = "some token",
                UserProfile = new UserProfile
                {
                    FirstName = "some",
                    LastName = "name",
                    Avatar = "someavatar"
                }
            };

            LogIn();

            _viewModel
                .ZwiftName
                .Should()
                .Be("some name");
        }

        [StaFact]
        public void GivenUserLoggedIn_ZwiftAvatarIsSet()
        {
            _windowService.LogInDialogResult = new TokenResponse
            {
                AccessToken = "some token",
                UserProfile = new UserProfile
                {
                    FirstName = "some",
                    LastName = "name",
                    Avatar = "someavatar"
                }
            };

            LogIn();

            _viewModel
                .ZwiftAvatarUri
                .Should()
                .Be("someavatar");
        }

        [StaFact]
        public void GivenUserLoggedIn_LoggedInToZwiftIsSet()
        {
            _windowService.LogInDialogResult = new TokenResponse
            {
                AccessToken = "some token",
                UserProfile = new UserProfile
                {
                    FirstName = "some",
                    LastName = "name",
                    Avatar = "someavatar"
                }
            };

            LogIn();

            _viewModel
                .LoggedInToZwift
                .Should()
                .BeTrue();
        }

        [StaFact]
        public void GivenUserLoggedIn_LoggedInStateIsDispatched()
        {
            _windowService.LogInDialogResult = new TokenResponse
            {
                AccessToken = "some token",
                UserProfile = new UserProfile
                {
                    FirstName = "some",
                    LastName = "name",
                    Avatar = "someavatar"
                }
            };

            LogIn();

            var lastState = GetFirstDispatchedGameState();

            lastState
                .Should()
                .BeOfType<LoggedInState>();
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

        private void LogIn()
        {
            _viewModel.LogInCommand.Execute(null);
        }
    }
}
