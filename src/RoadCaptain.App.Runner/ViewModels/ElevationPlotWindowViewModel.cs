// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Controls;
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
        private RenderMode _renderMode = RenderMode.All;

        public ElevationPlotWindowViewModel(ISegmentStore segmentStore, IWindowService windowService, IUserPreferences userPreferences)
        {
            _segmentStore = segmentStore;
            _windowService = windowService;
            _userPreferences = userPreferences;
            
            if (!string.IsNullOrEmpty(_userPreferences.ElevationPlotRenderMode) && Enum.TryParse<RenderMode>(_userPreferences.ElevationPlotRenderMode, out var renderMode))
            {
                _renderMode = renderMode;
            } 
        }

        public ICommand ToggleElevationPlotCommand => new AsyncRelayCommand(
            _ => ToggleElevationPlot(),
            _ => true);

        public ICommand ToggleRenderModeCommand => new AsyncRelayCommand(
            _ => ToggleRenderMode(),
            _ => true);

        private Task<CommandResult> ToggleRenderMode()
        {
            var renderMode = _renderMode;

            renderMode = renderMode switch
            {
                RenderMode.Unknown => RenderMode.All,
                RenderMode.All => RenderMode.Moving,
                RenderMode.Moving => RenderMode.MovingSegment,
                RenderMode.MovingSegment => RenderMode.AllSegment,
                RenderMode.AllSegment => RenderMode.All,
                _ => throw new ArgumentOutOfRangeException()
            };

            _userPreferences.ElevationPlotRenderMode = renderMode.ToString();
            _userPreferences.Save();
            
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

        public RenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                if (value == _renderMode) return;
                
                _renderMode = value;

                this.RaisePropertyChanged();
            }
        }

        public Brush BorderColor
        {
            get
            {
                var color = "#cccccc";
                
                switch (RenderMode)
                {
                    case RenderMode.Moving:
                        color = "#0000CC";
                        break;
                    case RenderMode.MovingSegment:
                    case RenderMode.AllSegment:
                        color = "#00CC00";
                        break;
                }

                return new SolidColorBrush(Color.Parse(color));
            }
        }

        private Task<CommandResult> ToggleElevationPlot()
        {
            _windowService.ToggleElevationPlot(null, false);

            return Task.FromResult(CommandResult.Success());
        }

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

