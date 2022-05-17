using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Models
{
    public class Marker
    {
        public string Id { get; set; }
        public SKPoint StartDrawPoint { get; set; }
        public SKPoint EndDrawPoint { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public string Name { get; set; }
        public SKPath Path { get; set; }
        public SegmentType Type { get; set; }
        public TrackPoint StartPoint { get; set; }
        public TrackPoint EndPoint { get; set; }
        public SKRect Bounds { get; set; }
    }
}