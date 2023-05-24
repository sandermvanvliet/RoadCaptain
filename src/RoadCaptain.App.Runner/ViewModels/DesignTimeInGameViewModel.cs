// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.GameStates;
using Serilog.Core;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeInGameViewModel : InGameNavigationWindowViewModel
    {
        private static readonly List<Segment> DefaultSegments = new()
        {
            new(new List<TrackPoint> { new TrackPoint(1, 1, 1, ZwiftWorldId.Watopia)})
            {
                Id = "seg-1",
                Name = "Segment 1",
                Sport = SportType.Cycling,
                Type = SegmentType.Segment
            },
            new(new List<TrackPoint> { new TrackPoint(1, 1, 1, ZwiftWorldId.Watopia)})
            {
                Id = "seg-2",
                Name = "Segment 2",
                Sport = SportType.Cycling,
                Type = SegmentType.Segment
            },
            new(new List<TrackPoint> { new TrackPoint(1, 1, 1, ZwiftWorldId.Watopia)})
            {
                Id = "seg-3",
                Name = "Segment 3",
                Sport = SportType.Cycling,
                Type = SegmentType.Segment
            }
        };

        public DesignTimeInGameViewModel()
            : base(
                new InGameWindowModel(DefaultSegments)
                {
                    Route = new PlannedRoute
                    {
                        Sport = SportType.Cycling, 
                        World = new World
                        {
                            Id = "watopia", 
                            Name = "Watopia"
                        },
                        Name = "Test route",
                        ZwiftRouteName = "The Mega Pretzel",
                        RouteSegmentSequence = new List<SegmentSequence>
                        {
                            new(segmentId: "seg-1", nextSegmentId: "seg-2", direction: SegmentDirection.AtoB,
                                turnToNextSegment: TurnDirection.GoStraight, type: SegmentSequenceType.Regular),
                            new(segmentId: "seg-2", nextSegmentId: "seg-3", direction: SegmentDirection.AtoB,
                                turnToNextSegment: TurnDirection.GoStraight, type: SegmentSequenceType.Regular),
                            new(segmentId: "seg-3", nextSegmentId: null, direction: SegmentDirection.AtoB,
                                turnToNextSegment: TurnDirection.GoStraight, type: SegmentSequenceType.Regular)
                        }
                    }
                }, 
                DefaultSegments, 
                null,
                new MonitoringEventsWithSerilog(Logger.None),
                null)
        {
            var onRouteState = new OnRouteState(1, 2, TrackPoint.Unknown, DefaultSegments[0], Model.Route, SegmentDirection.AtoB, 0, 0, 0);
            Model.Route.EnteredSegment("seg-1");
            UpdateGameState(onRouteState);
            //UpdateGameState(new ReadyToGoState());B
        }
    }
}
