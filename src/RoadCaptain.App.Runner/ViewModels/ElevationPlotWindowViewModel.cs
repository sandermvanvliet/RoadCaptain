// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        private TrackPoint? _riderPosition;
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;

        public ElevationPlotWindowViewModel(ISegmentStore segmentStore, IWindowService windowService, IUserPreferences userPreferences)
        {
            _segmentStore = segmentStore;
            _windowService = windowService;
            _userPreferences = userPreferences;

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

        public List<Segment>? Segments { get; private set; }

        public List<Segment>? Markers { get; private set; }

        public TrackPoint RiderPosition
        {
            get => _riderPosition ?? TrackPoint.Unknown;
            private set
            {
                _riderPosition = value;
                this.RaisePropertyChanged();
            }
        }

        public int ZoomWindowDistance
        {
            get => _userPreferences.ElevationPlotRangeInMeters.GetValueOrDefault(1000);
        }

        public bool ZoomOnCurrentPosition
        {
            get => _userPreferences.ElevationProfileZoomOnPosition.GetValueOrDefault();
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

