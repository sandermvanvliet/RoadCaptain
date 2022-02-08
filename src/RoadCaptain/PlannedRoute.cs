using System.Collections.Generic;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        public string StartingSegmentId { get; private set; } = "empty-segment";
        public string NextSegmentId { get; private set; } = "next-segment";
        public string CurrentSegment { get; private set; }
        public List<TurnDirection> TurnsToNextSegment { get; set; } = new();
        public TurnDirection TurnToNextSegment { get; set; } = TurnDirection.GoStraight;

        public void EnteredSegment(string segmentId)
        {
            if (CurrentSegment == null && segmentId == StartingSegmentId)
            {
                CurrentSegment = segmentId;
            }
        }
    }
}