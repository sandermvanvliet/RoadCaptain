namespace RoadCaptain
{
    public class SegmentSequence
    {
        public string SegmentId { get; set; }
        public TurnDirection TurnToNextSegment { get; set; }
        public string NextSegmentId { get; set; }
    }
}