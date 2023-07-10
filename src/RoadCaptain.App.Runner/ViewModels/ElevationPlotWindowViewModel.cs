// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
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
            parameter => ToggleRenderMode(parameter as RenderMode? ?? RenderMode.All),
            parameter => parameter is RenderMode);

        public ICommand ToggleKomZoomCommand => new AsyncRelayCommand(
            _ => ToggleKomZoom(),
            _ => true);

        private Task<CommandResult> ToggleRenderMode(RenderMode renderMode)
        {
            if (renderMode == RenderMode.All && RenderMode == RenderMode.MovingSegment)
            {
                renderMode = RenderMode.AllSegment;
            }
            if (renderMode == RenderMode.Moving && RenderMode == RenderMode.AllSegment)
            {
                renderMode = RenderMode.MovingSegment;
            }
            
            RenderMode = renderMode;
            
            return Task.FromResult(CommandResult.Success());
        }

        private Task<CommandResult> ToggleKomZoom()
        {
            var renderMode = RenderMode;
            
            renderMode = renderMode switch
            {
                RenderMode.All => RenderMode.AllSegment,
                RenderMode.Moving => RenderMode.MovingSegment,
                RenderMode.AllSegment => RenderMode.All,
                RenderMode.MovingSegment => RenderMode.Moving,
                _ => renderMode
            };

            RenderMode = renderMode;

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
                
                _userPreferences.ElevationPlotRenderMode = _renderMode.ToString();
                _userPreferences.Save();

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
                        color = "#FFCC00";
                        break;
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

