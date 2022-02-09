using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        private int _segmentIndex;
        private bool _hasStarted;
        private bool _hasCompleted;

        public string StartingSegmentId => RouteSegmentSequence[_segmentIndex].SegmentId;
        public string NextSegmentId => RouteSegmentSequence[_segmentIndex].NextSegmentId;
        public TurnDirection TurnToNextSegment => RouteSegmentSequence[_segmentIndex].TurnToNextSegment;
        public string CurrentSegmentId => _hasStarted ? RouteSegmentSequence[_segmentIndex].SegmentId : null;

        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        public void EnteredSegment(string segmentId)
        {
            if (_hasCompleted)
            {
                throw new ArgumentException("Route has already completed, can't enter new segment");
            }

            if (CurrentSegmentId == null && segmentId == StartingSegmentId)
            {
                _hasStarted = true;
            }
            else if (CurrentSegmentId != null && NextSegmentId == segmentId)
            {
                _segmentIndex++;
                
                // Use the segment index instead of comparing the segment
                // id with the id of the last segment because we may pass
                // the same segment multiple times in the course of this
                // route.
                if(_segmentIndex == RouteSegmentSequence.Count -1)
                {
                    _hasCompleted = true;
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