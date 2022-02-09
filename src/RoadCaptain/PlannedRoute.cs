using System;
using System.Collections.Generic;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        public bool HasCompleted { get; private set; }
        public bool HasStarted { get; private set; }

        public int SegmentSequenceIndex { get; private set; }

        public string StartingSegmentId => RouteSegmentSequence[SegmentSequenceIndex].SegmentId;
        public string NextSegmentId => RouteSegmentSequence[SegmentSequenceIndex].NextSegmentId;
        public TurnDirection TurnToNextSegment => RouteSegmentSequence[SegmentSequenceIndex].TurnToNextSegment;
        public string CurrentSegmentId => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].SegmentId : null;

        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        public void EnteredSegment(string segmentId)
        {
            if (HasCompleted)
            {
                throw new ArgumentException("Route has already completed, can't enter new segment");
            }

            if (CurrentSegmentId == null && segmentId == StartingSegmentId)
            {
                HasStarted = true;
            }
            else if (CurrentSegmentId != null && NextSegmentId == segmentId)
            {
                SegmentSequenceIndex++;

                // Use the segment index instead of comparing the segment
                // id with the id of the last segment because we may pass
                // the same segment multiple times in the course of this
                // route.
                if (SegmentSequenceIndex == RouteSegmentSequence.Count - 1)
                {
                    HasCompleted = true;
                }
            }
            else
            {
                throw new ArgumentException(
                    $"Was expecting {NextSegmentId} but got {segmentId} and that's not a valid route progression");
            }
        }
    }
}