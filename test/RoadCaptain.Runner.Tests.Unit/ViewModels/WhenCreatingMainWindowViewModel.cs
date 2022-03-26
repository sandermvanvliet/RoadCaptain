using FluentAssertions;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class WhenCreatingMainWindowViewModel
    {
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

        private static MainWindowViewModel CreateViewModel(Configuration configuration, AppSettings appSettings = null)
        {
            return new MainWindowViewModel(null, null, null, configuration, appSettings ?? new AppSettings());
        }
    }
}