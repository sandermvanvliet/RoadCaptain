using System.Collections.Generic;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        private readonly int _segmentIndex = 0;

        public string StartingSegmentId => RouteSegmentSequence[_segmentIndex].SegmentId;
        public string NextSegmentId => RouteSegmentSequence[_segmentIndex].NextSegmentId;
        public List<TurnDirection> TurnsToNextSegment => RouteSegmentSequence[_segmentIndex].ExpectedTurns;
        public TurnDirection TurnToNextSegment => RouteSegmentSequence[_segmentIndex].TurnToNextSegment;
        public string CurrentSegment { get; private set; }

        public List<SegmentSequence> RouteSegmentSequence { get; private set; }

        public void EnteredSegment(string segmentId)
        {
            if (CurrentSegment == null && segmentId == StartingSegmentId)
            {
                CurrentSegment = segmentId;
            }
        }

        public static PlannedRoute FixedForTesting()
        {
            var route = new PlannedRoute
            {
                RouteSegmentSequence = new List<SegmentSequence>
                {
                    new()
                    {
                        SegmentId = "watopia-bambino-fondo-004-before-after",
                        NextSegmentId = "watopia-bambino-fondo-004-after-after",
                        TurnToNextSegment = TurnDirection.GoStraight
                    },
                    new()
                    {
                        SegmentId = "watopia-bambino-fondo-004-after-after",
                        NextSegmentId = "watopia-bambino-fondo-004-before-after"
                    },
                    new()
                    {
                        SegmentId = "watopia-bambino-fondo-004-before-after",
                        NextSegmentId = "watopia-bambino-fondo-004-after-after"
                    }
                }
            };

            return route;
        }
    }

    public class SegmentSequence
    {
        public string SegmentId { get; set; }
        public List<TurnDirection> ExpectedTurns { get; set; }
        public TurnDirection TurnToNextSegment { get; set; }
        public string NextSegmentId { get; set; }
    }
}