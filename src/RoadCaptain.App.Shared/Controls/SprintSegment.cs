using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class SprintSegment : MarkerSegment
    {
        public SprintSegment(string id, SKPoint[] points)
            : base($"sprint-{id}", points, "#56A91D", "S", SkiaPaints.SprintSegmentPaint)
        {
        }
    }
}