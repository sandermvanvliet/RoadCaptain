using System.Collections.Generic;
using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.RouteBuilder.Models;
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
            Container.Resolve<IWorldStore>(),
            Container.Resolve<ISegmentStore>(),
            new MainWindowModel()
            )
        {
            Route.OutputFilePath = @"C:\git\RoadCaptain\test\RoadCaptain.Tests.Unit\GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json";
            Route.Load();
            Route.LoopMode = LoopMode.Infinite;
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