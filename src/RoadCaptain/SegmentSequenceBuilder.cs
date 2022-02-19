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

        public PlannedRoute Build()
        {
            return _route;
        }
    }
}