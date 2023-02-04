using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class ElevationPlotWindowViewModel : ViewModelBase
    {
        private readonly ISegmentStore _segmentStore;
        private TrackPoint _riderPosition;
        private readonly IWindowService _windowService;

        public ElevationPlotWindowViewModel(ISegmentStore segmentStore, IWindowService windowService)
        {
            _segmentStore = segmentStore;
            _windowService = windowService;

            ToggleElevationPlotCommand = new AsyncRelayCommand(
                _ => ToggleElevationPlot(),
                _ => true);
        }

        private Task<CommandResult> ToggleElevationPlot()
        {
            _windowService.ToggleElevationPlot(null, false);

            return Task.FromResult(CommandResult.Success());
        }

        public PlannedRoute? Route { get; private set; }

        public List<Segment> Segments { get; private set; }

        public List<Segment> Markers { get; private set; }

        public TrackPoint RiderPosition
        {
            get => _riderPosition;
            private set
            {
                _riderPosition = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand ToggleElevationPlotCommand { get; }

        public void UpdateGameState(GameState gameState)
        {
            switch (gameState)
            {
                case InGameState:
                    Route = null;
                    break;
                case OnRouteState onRouteState:
                    if (Route != onRouteState.Route)
                    {
                        UpdateRoute(onRouteState.Route);
                    }

                    RiderPosition = onRouteState.CurrentPosition;
                    break;
                case UpcomingTurnState upcomingTurnState:
                    RiderPosition = upcomingTurnState.CurrentPosition;
                    break;
            }
        }

        public void UpdateRoute(PlannedRoute route)
        {
            var segments = _segmentStore.LoadSegments(route.World!, route.Sport);
            var markers = _segmentStore.LoadMarkers(route.World!);

            Route = route;
            Segments = segments;
            Markers = markers;

            this.RaisePropertyChanged(nameof(Route));
            this.RaisePropertyChanged(nameof(Markers));
            this.RaisePropertyChanged(nameof(Segments));
        }
    }
}
