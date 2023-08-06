// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.Tests.Unit.GameState.Loops
{
    public class LoopStateTransitionTestBase : StateTransitionTestBase
    {
        protected static readonly TrackPoint RouteSegment0Point1 = new(7, 1, 0, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment0Point2 = new(7, 2, 1, ZwiftWorldId.Watopia);
        protected static readonly TrackPoint RouteSegment0Point3 = new(7, 3, 0, ZwiftWorldId.Watopia);

        protected static readonly Segment RouteSegment0 = new(new List<TrackPoint>
        {
            RouteSegment0Point1,
            RouteSegment0Point2,
            RouteSegment0Point3
        })
        {
            Id = "route-segment-0",
            Type = SegmentType.Segment,
            Sport = SportType.Cycling
        };

        protected override PlannedRoute Route { get; } = new()
        {
            RouteSegmentSequence =
            {
                new SegmentSequence(segmentId: RouteSegment0.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.LeadIn, nextSegmentId: RouteSegment1.Id, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(segmentId: RouteSegment1.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.LoopStart, nextSegmentId: RouteSegment2.Id, turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment2.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.Loop, nextSegmentId: RouteSegment3.Id, turnToNextSegment: TurnDirection.Left),
                new SegmentSequence(segmentId: RouteSegment3.Id, direction: SegmentDirection.AtoB, type: SegmentSequenceType.LoopEnd, nextSegmentId: RouteSegment0.Id, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(segmentId: RouteSegment0.Id, direction: SegmentDirection.BtoA, type: SegmentSequenceType.LeadOut)
            },
            Sport = SportType.Cycling,
            WorldId = "watopia",
            World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
            Name = "test",
            ZwiftRouteName = "zwift test route name",
            NumberOfLoops = 1
        };

        public LoopStateTransitionTestBase()
        {
            Segments = new List<Segment>
            {
                Segment1,
                Segment2,
                Segment3,
                RouteSegment0,
                RouteSegment1,
                RouteSegment2,
                RouteSegment3,
            };

            // This is not done automatically by the base class...
            RouteSegment0.CalculateDistances();
            
            // Note: Because the segments are static fields we must only
            //       initialize the turns once.
            lock (RouteSegment0)
            {
                if (!RouteSegment0.NextSegmentsNodeB.Any())
                {
                    RouteSegment0.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, RouteSegment3.Id));
                }
            }

            // Adjust route segment 3 so that it can go to route segment 0
            lock (RouteSegment3)
            {
                RouteSegment3.NextSegmentsNodeB.Clear();
                RouteSegment3.NextSegmentsNodeB.Add(new Turn(TurnDirection.Left, Segment2.Id));
                RouteSegment3.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, RouteSegment0.Id));
            }
        }
    }
}

