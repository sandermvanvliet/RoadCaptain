namespace RoadCaptain
{
    public class SegmentSequence
    {
        public string SegmentId { get; set; }
        public TurnDirection TurnToNextSegment { get; set; } = TurnDirection.None;
        public string NextSegmentId { get; set; }
        public SegmentDirection Direction { get; set; } = SegmentDirection.Unknown;
    }
}