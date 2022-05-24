using System;
using System.Configuration;
using System.Threading;
using FluentAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.GameStates;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
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
            var appSettings = new DummyUserPreferences
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
        public void GivenConfigurationContainsAccessToken_ZwiftAccessTokenIsSet()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = ValidToken()
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
                AccessToken = ValidToken()
            };
            
            CreateViewModel(configuration)
                .ZwiftAvatarUri
                .Should()
                .Be("avares://RoadCaptain.App.Shared/Assets/profile-default.png");
        }

        [Fact]
        public void GivenConfigurationContainsAccessToken_ZwiftNameIsSetToStoredToken()
        {
            var configuration = new Configuration(null)
            {
                AccessToken = ValidToken()
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
                AccessToken = ValidToken()
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
                AccessToken = ValidToken(),
                Route = "someroute.json"
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
                AccessToken = ValidToken()
            };

            CreateViewModel(configuration);

            GetFirstDispatchedGameState()
                .Should()
                .BeOfType<LoggedInState>();
        }

        private static MainWindowViewModel CreateViewModel(Configuration configuration, IUserPreferences appSettings = null)
        {
            
            var routeStore = new StubRouteStore();
            return new MainWindowViewModel(configuration, 
                appSettings ?? new DummyUserPreferences(),
                new WindowService(null, new NopMonitoringEvents()),
                _gameStateDispatcher,
                new LoadRouteUseCase(_gameStateDispatcher, routeStore),
                routeStore,
                null);
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