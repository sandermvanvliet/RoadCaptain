using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class ClimbSegment : MarkerSegment
    {
        public ClimbSegment(string id, SKPoint[] points)
            : base($"climb-{id}", points, "#fc4119", "K", SkiaPaints.ClimbSegmentPaint)
        {
        }
    }
}