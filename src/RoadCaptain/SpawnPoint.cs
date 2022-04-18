namespace RoadCaptain
{
    public class SpawnPoint
    {
        public string SegmentId { get; set; }
        public string ZwiftRouteName { get; set; }
        public SegmentDirection Direction { get; set; }
        public SportType Sport { get; set; }
    }
}