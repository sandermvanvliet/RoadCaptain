using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class WhenCallingLoadRouteCommand
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly StubWindowService _windowService;

        public WhenCallingLoadRouteCommand()
        {
            _windowService = new StubWindowService(null, null);

            var gameStateDispatcher = new InMemoryGameStateDispatcher(new NopMonitoringEvents());

            var routeStore = new StubRouteStore();
            _viewModel = new MainWindowViewModel(new Configuration(null),
                new DummyUserPreferences(),
                _windowService,
                null,
                new LoadRouteUseCase(gameStateDispatcher, routeStore),
                routeStore,
                null);
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
        public void GivenUserSelectedFile_RouteIsLoaded()
        {
            _viewModel.RoutePath = null;
            _windowService.OpenFileDialogResult = "someroute.json";
            
            LoadRoute();

            _viewModel.Route.Should().NotBeNull();
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
