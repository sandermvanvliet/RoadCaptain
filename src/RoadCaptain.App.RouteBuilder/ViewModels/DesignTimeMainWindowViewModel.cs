// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.App.Shared.ViewModels;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Serilog.Core;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeMainWindowViewModel : MainWindowViewModel
    {
        private static readonly IContainer Container;

        static DesignTimeMainWindowViewModel()
        {
            Container = InversionOfControl.ConfigureContainer(new ConfigurationBuilder().Build(), Logger.None, Dispatcher.UIThread).Build();
        }

        public DesignTimeMainWindowViewModel()
        : base(Container.Resolve<IRouteStore>(), 
            Container.Resolve<ISegmentStore>(), 
            new DummyVersionChecker(), 
            new WindowService(Container, Container.Resolve<MonitoringEvents>()), 
            Container.Resolve<IWorldStore>(), 
            new DummyUserPreferences(),
            new DummyApplicationFeatures(),
            new StatusBarService(),
            Container.Resolve<SearchRoutesUseCase>(),
            Container.Resolve<LoadRouteFromFileUseCase>(), 
            Container.Resolve<DeleteRouteUseCase>())
        {
            // Route.OutputFilePath = @"C:\git\RoadCaptain\test\RoadCaptain.Tests.Unit\GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json";
            // Route.Load();
            // Route.LoopMode = LoopMode.Infinite;
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
            // Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>()){ Id = "test", Name="Test", Type = SegmentType.Climb, Sport = SportType.Cycling}));
        }
    }

    public class DummyApplicationFeatures : IApplicationFeatures
    {
        public bool IsPreRelease { get; set; }
    }

    public class DummyVersionChecker : IVersionChecker
    {
        public (Release? official, Release? preRelease) GetLatestRelease()
        {
            return (null, null);
        }
    }
}
