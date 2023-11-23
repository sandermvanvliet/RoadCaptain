// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Net.Http;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using Moq;
using RoadCaptain.Adapters;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.GameStates;
using Xunit;
using TokenResponse = RoadCaptain.App.Shared.Models.TokenResponse;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCallingLogInCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly StubWindowService _windowService;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly InMemoryZwiftCredentialCache _credentialCache;

        public WhenCallingLogInCommand()
        {
            _windowService = new StubWindowService();
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents(), new PlatformPaths());
            _credentialCache = new InMemoryZwiftCredentialCache();

            StubRouteStore routeStore = new StubRouteStore();
            _viewModel = new MainWindowViewModel(new Configuration(null),
                new DummyUserPreferences(),
                _windowService,
                _gameStateDispatcher,
                routeStore,
                new StubVersionChecker(), 
                new SegmentStore(),
                _credentialCache,
                new NopMonitoringEvents(),
                new DummyApplicationFeatures(),
                new Zwift(null!));
            
            // This is required so that we can call new Window() below.
            var avaloniaDependencyResolver = new AvaloniaLocator();
            var mock = new Mock<IWindowingPlatform>();
            mock.Setup(_ => _.CreateWindow()).Returns(new Mock<IWindowImpl>().Object);
            avaloniaDependencyResolver.Bind<IWindowingPlatform>().ToConstant(mock.Object);
            AvaloniaLocator.Current = avaloniaDependencyResolver;
        }

        [Fact]
        public void LogInDialogIsOpened()
        {
            LogIn();

            _windowService
                .LogInDialogInvocations
                .Should()
                .Be(1);
        }

        [Fact]
        public void GivenLogInDialogIsCanceled_ZwiftAccessTokenRetainsOriginalValue()
        {
            _windowService.LogInDialogResult = null;

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenUserLogsInButTokenIsEmpty_ZwiftAccessTokenRetainsOriginalValue()
        {
            _windowService.LogInDialogResult = new TokenResponse();

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenUserLoggedIn_ZwiftAccessTokenIsSet()
        {
            _windowService.LogInDialogResult = new TokenResponse { AccessToken = "some token", UserProfile = new UserProfile() };

            LogIn();

            _viewModel
                .ZwiftAccessToken
                .Should()
                .Be("some token");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void GivenUserLoggedIn_CredentialsAreCached()
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

            _credentialCache
                .LoadAsync()
                .GetAwaiter()
                .GetResult()
                .Should()
                .NotBeNull();
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

        private void LogIn()
        {
            _viewModel.LogInCommand.Execute(new Window());
        }
    }
}

