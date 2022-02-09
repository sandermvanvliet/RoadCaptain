using System.Linq;

namespace RoadCaptain.Tests.Unit
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

        public SegmentSequenceBuilder TuringLeftTo(string segmentId)
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