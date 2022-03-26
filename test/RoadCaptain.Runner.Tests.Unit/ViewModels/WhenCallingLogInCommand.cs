using FluentAssertions;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class WhenCallingLogInCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly StubWindowService _windowService;

        public WhenCallingLogInCommand()
        {
            _windowService = new StubWindowService();

            _viewModel = new MainWindowViewModel(
                null,
                null,
                new Configuration(null),
                new AppSettings(),
                _windowService);
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
            _windowService.LogInDialogResult = new TokenResponse { AccessToken = "some token" };

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

        private void LogIn()
        {
            _viewModel.LogInCommand.Execute(null);
        }
    }
}
