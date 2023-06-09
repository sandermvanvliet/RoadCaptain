// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Net.Http;
using System.Threading;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.GameStates;
using Xunit;
using TokenResponse = RoadCaptain.App.Shared.Models.TokenResponse;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCreatingMainWindowViewModel
    {
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly IZwiftCredentialCache _credentialCache;

        public WhenCreatingMainWindowViewModel()
        {
            _gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents(), new PlatformPaths());
            _credentialCache = new InMemoryZwiftCredentialCache();
        }

        [Fact]
        public void GivenConfigurationContainsPathToRouteAndFileDoesNotExist_RoutePathIsNotSet()
        {
            var routePath = "Some route path";
            var configuration = new Configuration(null)
            {
                Route = routePath
            };

            CreateViewModel(configuration)
                .RoutePath
                .Should()
                .BeNullOrEmpty();
        }

        [Fact]
        public void GivenConfigurationDoesNotHaveRoutePathAndAppSettingsHasRoutePathAndFileDoesNotExist_RoutePathIsNotSet()
        {
            var appSettings = new DummyUserPreferences
            {
                Route = "Some route path"
            };
            var configuration = new Configuration(null);

            CreateViewModel(configuration, appSettings)
                .RoutePath
                .Should()
                .BeNullOrEmpty();
        }

        [Fact]
        public void GivenConfigurationContainsPathToRouteAndFileExists_RoutePathIsSet()
        {
            var routePath = "someroute.json";
            var configuration = new Configuration(null)
            {
                Route = routePath
            };

            CreateViewModel(configuration)
                .RoutePath
                .Should()
                .Be("someroute.json");
        }

        [Fact]
        public void GivenConfigurationDoesNotHaveRoutePathAndAppSettingsHasRoutePathAndFileExists_RoutePathIsSet()
        {
            var appSettings = new DummyUserPreferences
            {
                Route = "someroute.json"
            };
            var configuration = new Configuration(null);

            CreateViewModel(configuration, appSettings)
                .RoutePath
                .Should()
                .Be("someroute.json");
        }

        [Fact]
        public void GivenNoRoutePathInConfigurationOrSettings_RoutePathIsNull()
        {
            var configuration = new Configuration(null);
            var appSettings = new DummyUserPreferences { Route = null };

            CreateViewModel(configuration, appSettings)
                .RoutePath
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenNoCachedCredentials_ZwiftAvatarUriIsSetToDefaultImage()
        {
            var configuration = new Configuration(null);
            
            CreateViewModel(configuration)
                .ZwiftAvatarUri
                .Should()
                .Be("avares://RoadCaptain.App.Shared/Assets/profile-default.png");
        }

        [Fact]
        public void GivenCachedCredentials_ZwiftNameIsSetToStoredToken()
        {
            var configuration = new Configuration(null);
            GivenCachedCredentials();

            CreateViewModel(configuration)
                .ZwiftName
                .Should()
                .Be("first last");
        }

        [Fact]
        public void GivenCachedCredentials_LoggedInToZwiftIsTrue()
        {
            var configuration = new Configuration(null)
            {
            };
            GivenCachedCredentials();
            
            CreateViewModel(configuration)
                .LoggedInToZwift
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenCachedCredentialsAndRouteSetInConfiguration_CanStartRouteIsTrue()
        {
            var configuration = new Configuration(null)
            {
                Route = "someroute.json"
            };
            GivenCachedCredentials();
            
            CreateViewModel(configuration)
                .CanStartRoute
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenCachedCredentials_LoggedInStateIsDispatched()
        {
            var configuration = new Configuration(null);
            GivenCachedCredentials();

            CreateViewModel(configuration);

            GetFirstDispatchedGameState()
                .Should()
                .BeOfType<LoggedInState>();
        }

        private void GivenCachedCredentials()
        {
            _credentialCache.StoreAsync(new TokenResponse
            {
                AccessToken = ValidToken(),
                UserProfile = new UserProfile { FirstName = "first", LastName = "last", Avatar = "avatar.png" }
            });
        }

        private MainWindowViewModel CreateViewModel(Configuration configuration, IUserPreferences? appSettings = null)
        {
            var routeStore = new StubRouteStore();

            var mainWindowViewModel = new MainWindowViewModel(configuration, 
                appSettings ?? new DummyUserPreferences(),
                new StubWindowService(),
                _gameStateDispatcher,
                routeStore,
                new StubVersionChecker(),
                new SegmentStore(),
                _credentialCache,
                new NopMonitoringEvents(),
                new DummyApplicationFeatures());

            mainWindowViewModel.Initialize().GetAwaiter().GetResult();
            
            return mainWindowViewModel;
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

        private static string ValidToken()
        {
            var handler = new JsonWebTokenHandler();

            var token = handler.CreateToken(JsonConvert.SerializeObject(new
            {
                exp = DateTimeOffset.UtcNow.AddHours(3).ToUnixTimeSeconds()
            }));

            return token;
        }
    }
}
