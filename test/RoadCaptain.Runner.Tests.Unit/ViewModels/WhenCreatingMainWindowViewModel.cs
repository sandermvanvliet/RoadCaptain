﻿using System;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class WhenCreatingMainWindowViewModel
    {
        private static InMemoryGameStateDispatcher _gameStateDispatcher;

        public WhenCreatingMainWindowViewModel()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents());
        }

        [Fact]
        public void GivenConfigurationContainsPathToRoute_RoutePathIsSet()
        {
            var routePath = "Some route path";
            var configuration = new Configuration(null)
            {
                Route = routePath
            };

            CreateViewModel(configuration)
                .RoutePath
                .Should()
                .Be(routePath);
        }

        [Fact]
        public void GivenConfigurationDoesNotHaveRoutePathAndAppSettingsHasRoutePath_RoutePathIsSet()
        {
            var appSettings = new AppSettings
            {
                Route = "Some route path"
            };
            var configuration = new Configuration(null);
            
            CreateViewModel(configuration, appSettings)
                .RoutePath
                .Should()
                .Be("Some route path");
        }

        [Fact]
        public void GivenNoRoutePathInConfigurationorSettings_RoutePathIsNull()
        {
            var configuration = new Configuration(null);
            
            CreateViewModel(configuration)
                .RoutePath
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_ZwiftAccessTokenIsSet()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token"
            };
            
            CreateViewModel(configuration)
                .ZwiftAccessToken
                .Should()
                .Be(configuration.AccessToken);
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_ZwiftAvatarUriIsSetToDefaultImage()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token"
            };
            
            CreateViewModel(configuration)
                .ZwiftAvatarUri
                .Should()
                .Be("Assets/profile-default.png");
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_ZwiftNameIsSetToStoredToken()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token"
            };
            
            CreateViewModel(configuration)
                .ZwiftName
                .Should()
                .Be("(stored token)");
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_LoggedInToZwiftIsTrue()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token"
            };
            
            CreateViewModel(configuration)
                .LoggedInToZwift
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenConfigurationContainsRoutePathAndAccessToken_CanStartRouteIsTrue()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token",
                Route = "some route"
            };
            
            CreateViewModel(configuration)
                .CanStartRoute
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_LoggedInStateIsDispatched()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = "some token"
            };

            CreateViewModel(configuration);

            GetFirstDispatchedGameState()
                .Should()
                .BeOfType<LoggedInState>();
        }

        private static MainWindowViewModel CreateViewModel(Configuration configuration, AppSettings appSettings = null)
        {
            return new MainWindowViewModel(
                null, 
                null, 
                configuration, 
                appSettings ?? new AppSettings(),
                new WindowService(null),
                _gameStateDispatcher);
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
    }
}