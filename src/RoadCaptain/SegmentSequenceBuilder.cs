// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;

namespace RoadCaptain
{
    public class SegmentSequenceBuilder
    {
        private readonly PlannedRoute _route;

        public SegmentSequenceBuilder()
        {
            _route = new PlannedRoute();
        }

        private SegmentSequence Last => _route.RouteSegmentSequence.Last();

        public SegmentSequenceBuilder StartingAt(string segmentId)
        {
            var step = new SegmentSequence
            {
                SegmentId = segmentId
            };

            _route.RouteSegmentSequence.Add(step);

            return this;
        }

        public SegmentSequenceBuilder TurningLeftTo(string segmentId)
        {
            Last.NextSegmentId = segmentId;
            Last.TurnToNextSegment = TurnDirection.Left;

            var step = new SegmentSequence
            {
                SegmentId = segmentId
            };

            _route.RouteSegmentSequence.Add(step);
            
            return this;
        }

        public SegmentSequenceBuilder GoingStraightTo(string segmentId)
        {
            Last.NextSegmentId = segmentId;
            Last.TurnToNextSegment = TurnDirection.GoStraight;

            var step = new SegmentSequence
            {
                SegmentId = segmentId
            };

            _route.RouteSegmentSequence.Add(step);

            return this;
        }

        public SegmentSequenceBuilder TurningRightTo(string segmentId)
        {
            Last.NextSegmentId = segmentId;
            Last.TurnToNextSegment = TurnDirection.Right;

            var step = new SegmentSequence
            {
                SegmentId = segmentId
            };

            _route.RouteSegmentSequence.Add(step);

            return this;
        }

        public SegmentSequenceBuilder EndingAt(string segmentId)
        {
            if (Last.SegmentId != segmentId)
            {
                throw new ArgumentException(
                    "Can't end on a segment that the route did not enter. Did you call any of the turns?");
            }

            Last.NextSegmentId = null;
            Last.TurnToNextSegment = TurnDirection.None;
            
            return this;
        }

        public SegmentSequenceBuilder Loop()
        {
            Last.NextSegmentId = _route.RouteSegmentSequence.First().SegmentId;

            foreach (var sequence in _route.RouteSegmentSequence)
            {
                sequence.Type = SegmentSequenceType.Loop;
            }

            return this;
        }

        public PlannedRoute Build()
        {
            return _route;
        }

        public static PlannedRoute TestLoopOne()
        {
            return new SegmentSequenceBuilder()
                .StartingAt("watopia-bambino-fondo-001-after-after-after-after-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 1
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 2
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 3
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .TurningRightTo("watopia-bambino-fondo-004-before-before")
                // Around the volcano
                .TurningRightTo("watopia-bambino-fondo-004-before-after")
                .TurningRightTo("watopia-beach-island-loop-001")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .TurningRightTo("watopia-bambino-fondo-001-after-after-before-after")
                // Start the cliffside loop
                .TurningRightTo("watopia-bambino-fondo-003-before-before")
                .TurningLeftTo("watopia-big-loop-rev-001-before-before")
                .TurningLeftTo("watopia-ocean-lava-cliffside-loop-001")
                .TurningLeftTo("watopia-big-loop-rev-001-after-after")
                .EndingAt("watopia-big-loop-rev-001-after-after")
                .Build();
        }

        public static PlannedRoute TestLoopTwo()
        {
            return new SegmentSequenceBuilder()
                .StartingAt("watopia-bambino-fondo-001-after-after-after-after-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-before-before-before")
                .TurningRightTo("watopia-climbers-gambit-001")
                .TurningRightTo("watopia-bambino-fondo-002-before-after")
                .TurningRightTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .GoingStraightTo("watopia-bambino-fondo-002-before-after")
                .GoingStraightTo("watopia-bambino-fondo-002-before-before-after")
                .GoingStraightTo("watopia-bambino-fondo-002-before-before-before")
                .TurningRightTo("watopia-bambino-fondo-001-after-after-before-before-before")
                .TurningLeftTo("watopia-big-foot-hills-001-after-after")
                .TurningRightTo("watopia-big-foot-hills-001-after-before")
                .TurningRightTo("watopia-big-foot-hills-003-before")
                .TurningLeftTo("watopia-big-loop-rev-001-after-after")
                .TurningRightTo("watopia-ocean-lava-cliffside-loop-001")
                .EndingAt("watopia-ocean-lava-cliffside-loop-001")
                .Build();
        }
    }
}
