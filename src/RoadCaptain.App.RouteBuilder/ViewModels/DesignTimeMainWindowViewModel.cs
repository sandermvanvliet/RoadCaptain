using System.Collections.Generic;
using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.Ports;
using Serilog.Core;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeMainWindowViewModel : MainWindowViewModel
    {
        private static readonly IContainer _container;

        static DesignTimeMainWindowViewModel()
        {
            _container = InversionOfControl.ConfigureContainer(new ConfigurationBuilder().Build(), Logger.None, Dispatcher.UIThread).Build();
        }

        public DesignTimeMainWindowViewModel()
        : base(_container.Resolve<IRouteStore>(), 
            _container.Resolve<ISegmentStore>(), 
            new DummyVersionChecker(), 
            new WindowService(null, _container.Resolve<MonitoringEvents>()), 
            _container.Resolve<IWorldStore>(), 
            new DummyUserPreferences())
        {
            Route.OutputFilePath = @"C:\git\RoadCaptain\test\RoadCaptain.Tests.Unit\GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json";
            Route.Load();
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

    public class DummyVersionChecker : IVersionChecker
    {
        public Release GetLatestRelease()
        {
            return null;
        }
    }
}