using FluentAssertions;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class WhenCallingLoadRouteCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly StubWindowService _windowService;

        public WhenCallingLoadRouteCommand()
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
        public void OpenFileDialogIsOpened()
        {
            LoadRoute();

            _windowService
                .OpenFileDialogInvocations
                .Should()
                .Be(1);
        }

        [Fact]
        public void GivenOpenFileDialogIsCanceled_RoutePathRetainsOriginalValue()
        {
            _viewModel.RoutePath = "some path";

            _windowService.OpenFileDialogResult = null;

            LoadRoute();

            _viewModel
                .RoutePath
                .Should()
                .Be("some path");
        }

        [Fact]
        public void GivenUserSelectedFile_RoutePathIsSet()
        {
            _viewModel.RoutePath = null;
            _windowService.OpenFileDialogResult = "some path";

            LoadRoute();

            _viewModel
                .RoutePath
                .Should()
                .Be("some path");
        }

        [Fact]
        public void GivenUserSelectedFile_WindowTitleIsUpdatedWithRouteFileName()
        {
            _viewModel.RoutePath = null;
            _windowService.OpenFileDialogResult = "c:\\some\\route.json";

            LoadRoute();

            _viewModel
                .WindowTitle
                .Should()
                .Be("RoadCaptain - route.json");
        }

        private void LoadRoute()
        {
            _viewModel.LoadRouteCommand.Execute(null);
        }
    }
}
