// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.Ports;
using Serilog.Core;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeRouteSegmentListViewModel : RouteSegmentListViewModel
    {
        private static readonly IContainer Container;

        static DesignTimeRouteSegmentListViewModel()
        {
            Container = InversionOfControl
                .ConfigureContainer(new ConfigurationBuilder().Build(), Logger.None, Dispatcher.UIThread).Build();
        }

        public DesignTimeRouteSegmentListViewModel()
            : base(new RouteViewModel(
                    Container.Resolve<IRouteStore>(),
                    Container.Resolve<ISegmentStore>()),
                new DesignTimeWindowService())
        {
            Route.OutputFilePath =
                @"C:\git\RoadCaptain\test\RoadCaptain.Tests.Unit\GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json";
            Route.Load();
            Route.LoopMode = LoopMode.Constrained;
            Route.NumberOfLoops = 5;
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
            Route.Markers.Add(new MarkerViewModel(new Segment(new List<TrackPoint>())
                { Id = "test", Name = "Test", Type = SegmentType.Climb, Sport = SportType.Cycling }));
        }
    }
}
