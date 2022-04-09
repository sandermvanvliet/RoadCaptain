namespace RoadCaptain.RouteBuilder.ViewModels
{
    internal class SpawnPoint
    {
        public string SegmentId { get; }
        public string ZwiftRouteName { get; }
        public SegmentDirection SegmentDirection { get; }

        public SpawnPoint(string segmentId, string zwiftRouteName, SegmentDirection segmentDirection)
        {
            SegmentId = segmentId;
            ZwiftRouteName = zwiftRouteName;
            SegmentDirection = segmentDirection;
        }
    }
}