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
                            new()
                            {
                                SegmentId = "seg-1",
                                NextSegmentId = "seg-2",
                                Direction = SegmentDirection.AtoB,
                                TurnToNextSegment = TurnDirection.GoStraight,
                                Type = SegmentSequenceType.Regular
                            },
                            new()
                            {
                                SegmentId = "seg-2",
                                NextSegmentId = "seg-3",
                                Direction = SegmentDirection.AtoB,
                                TurnToNextSegment = TurnDirection.GoStraight,
                                Type = SegmentSequenceType.Regular
                            },
                            new()
                            {
                                SegmentId = "seg-3",
                                NextSegmentId = null,
                                Direction = SegmentDirection.AtoB,
                                TurnToNextSegment = TurnDirection.GoStraight,
                                Type = SegmentSequenceType.Regular
                            }
                        }
                    }
                }, 
                DefaultSegments, 
                null,
                new MonitoringEventsWithSerilog(Logger.None))
        {
            var onRouteState = new OnRouteState(1, 2, TrackPoint.Unknown, DefaultSegments[0], Model.Route, SegmentDirection.AtoB, 0, 0, 0);
            Model.Route.EnteredSegment("seg-1");
            UpdateGameState(onRouteState);
            //UpdateGameState(new ReadyToGoState());B
        }
    }
}