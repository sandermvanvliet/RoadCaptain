// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.Tests.Unit.GameState
{
    public class StateTransitionTestBase
    {
        protected static readonly TrackPoint PointNotOnAnySegment = new(0, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment1Point1 = new(1, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment1Point2 = new(1, 2, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment2Point1 = new(2, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment2Point2 = new(2, 2, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment3Point1 = new(6, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint Segment3Point2 = new(6, 2, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment1Point1 = new(3, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment1Point2 = new(3, 2, 1, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment1Point3 = new(3, 3, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment2Point1 = new(4, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment2Point2 = new(4, 2, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment2Point3 = new(4, 3, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment3Point1 = new(5, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment3Point2 = new(5, 2, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment3Point3 = new(5, 3, 0, ZwiftWorldId.Watopia);

        protected static readonly Segment Segment1 = new(new List<TrackPoint> { Segment1Point1, Segment1Point2 })
        {
            Id = "segment-1",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling,
        };

        protected static readonly Segment Segment2 = new(new List<TrackPoint> { Segment2Point1, Segment2Point2 })
        {
            Id = "segment-2",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected static readonly Segment Segment3 = new(new List<TrackPoint> { Segment3Point1, Segment3Point2 })
        {
            Id = "segment-3",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected static readonly Segment RouteSegment1 = new(new List<TrackPoint>
        {
            RouteSegment1Point1,
            RouteSegment1Point2,
            RouteSegment1Point3
        })
        {
            Id = "route-segment-1",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected static readonly Segment RouteSegment2 = new(new List<TrackPoint>
        {
            RouteSegment2Point1,
            RouteSegment2Point2,
            RouteSegment2Point3
        })
        {
            Id = "route-segment-2",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected static readonly Segment RouteSegment3 = new(new List<TrackPoint>
        {
            RouteSegment3Point1,
            RouteSegment3Point2,
            RouteSegment3Point3
        })
        {
            Id = "route-segment-3",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected virtual PlannedRoute Route { get; } = new()
        {
            RouteSegmentSequence =
            {
                new SegmentSequence(segmentId: RouteSegment1.Id, direction: SegmentDirection.AtoB,
                    type: SegmentSequenceType.Regular, nextSegmentId: RouteSegment2.Id,
                    turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment2.Id, direction: SegmentDirection.AtoB,
                    type: SegmentSequenceType.Regular, nextSegmentId: RouteSegment3.Id,
                    turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment3.Id, direction: SegmentDirection.AtoB,
                    type: SegmentSequenceType.Regular)
            },
            Sport = SportType.Cycling,
            WorldId = "watopia",
            World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
            Name = "test",
            ZwiftRouteName = "zwift test route name"
        };

        protected List<Segment> Segments = new()
        {
            Segment1,
            Segment2,
            Segment3,
            RouteSegment1,
            RouteSegment2,
            RouteSegment3
        };

        public StateTransitionTestBase()
        {
            // Note: Because the segments are static fields we must only
            //       initialize the turns once.
            lock (RouteSegment1)
            {
                if (!RouteSegment1.NextSegmentsNodeA.Any())
                {
                    RouteSegment1.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, Segment1.Id));
                    RouteSegment1.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, RouteSegment2.Id));
                    RouteSegment1.NextSegmentsNodeB.Add(new Turn(TurnDirection.Left, Segment2.Id));
                }
            }
            lock (RouteSegment2)
            {
                if (!RouteSegment2.NextSegmentsNodeA.Any())
                {
                    RouteSegment2.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, RouteSegment1.Id));
                    RouteSegment2.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, RouteSegment3.Id));
                    RouteSegment2.NextSegmentsNodeB.Add(new Turn(TurnDirection.Left, Segment1.Id));
                    RouteSegment2.NextSegmentsNodeB.Add(new Turn(TurnDirection.Right, Segment2.Id));
                }
            }

            lock (RouteSegment3)
            {
                if (!RouteSegment3.NextSegmentsNodeA.Any())
                {
                    RouteSegment3.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, RouteSegment2.Id));
                    RouteSegment3.NextSegmentsNodeB.Add(new Turn(TurnDirection.Left, Segment2.Id));
                    RouteSegment3.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, Segment1.Id));
                }
            }

            foreach (var segment in Segments)
            {
                segment.CalculateDistances();
            }
        }

        protected PlannedRoute GivenPlannedRoute()
        {
            return new()
            {
                RouteSegmentSequence =
                {
                    new SegmentSequence(segmentId: RouteSegment1.Id, direction: SegmentDirection.AtoB,
                        type: SegmentSequenceType.Regular, nextSegmentId: RouteSegment2.Id,
                        turnToNextSegment: TurnDirection.Left),
                    new SegmentSequence(segmentId: RouteSegment2.Id, direction: SegmentDirection.AtoB,
                        type: SegmentSequenceType.Regular, nextSegmentId: RouteSegment3.Id,
                        turnToNextSegment: TurnDirection.Left),
                    new SegmentSequence(segmentId: RouteSegment3.Id, direction: SegmentDirection.AtoB,
                        type: SegmentSequenceType.Regular)
                },
                Sport = SportType.Cycling,
                WorldId = "watopia",
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                Name = "test",
                ZwiftRouteName = "zwift test route name"
            };
        }
    }
}
