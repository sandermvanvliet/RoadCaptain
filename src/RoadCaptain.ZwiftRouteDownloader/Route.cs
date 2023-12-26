namespace RoadCaptain.ZwiftRouteDownloader
{
    public class Route
    {
        public long? StravaSegmentId { get; set; }
        public string Name => Slug;
        public string Slug { get; set; }
        public string World => WhatsOnZwiftUrl?.Replace("https://whatsonzwift.com/world/", "").Split("/")[0];
        public string StravaSegmentUrl { get; set; }
        public string[] Sports { get; set; } = { "Cycling" };
        public string WhatsOnZwiftUrl { get; set; }
    }
}