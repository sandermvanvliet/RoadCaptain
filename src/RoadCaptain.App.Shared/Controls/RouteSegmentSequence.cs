namespace RoadCaptain.App.Shared.Controls
{
    public class RouteSegmentSequence
    {
        public SegmentDirection Direction { get; set; }
        public string SegmentId { get; set; }
        public SegmentSequenceType Type { get; set; }
        public bool IsLeadIn => Type == SegmentSequenceType.LeadIn;
    }
}