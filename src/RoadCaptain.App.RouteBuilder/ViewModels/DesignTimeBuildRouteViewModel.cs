// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.Ports;
using Serilog.Core;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeBuildRouteViewModel : BuildRouteViewModel
    {
        private static readonly IContainer Container;

        static DesignTimeBuildRouteViewModel()
        {
            var builder = InversionOfControl.ConfigureContainer(new ConfigurationBuilder().Build(), Logger.None, Dispatcher.UIThread);
            
            builder.Register<IUserPreferences>(_ => new DummyUserPreferences()).SingleInstance();
            
            Container = builder.Build();
        }
        
        public DesignTimeBuildRouteViewModel()
        : base(new RouteViewModel(Container.Resolve<IRouteStore>(), Container.Resolve<ISegmentStore>()),
            new DummyUserPreferences(),
            new WindowService(Container, Container.Resolve<MonitoringEvents>()),
            Container.Resolve<ISegmentStore>(),
            new StatusBarService()
            )
        {
            var plannedRoute = Container.Resolve<IRouteStore>().LoadFrom(@"C:\git\RoadCaptain\test\RoadCaptain.Tests.Unit\GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json");
            Route.LoadFromRouteModel(new RouteModel
            {
                PlannedRoute = plannedRoute
            });
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
        }
    }
}
