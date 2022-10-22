using Codenizer.Avalonia.Map;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public class MapSegment : MapObject
    {
        private readonly SKPath _path;
        public bool IsSpawnPoint { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsOnRoute { get; set; }
        public bool IsLeadIn { get; set; }
        public bool IsLeadOut { get; set; }

        public MapSegment(string segmentId, SKPoint[] points, bool isSpawnPoint)
        {
            IsSpawnPoint = isSpawnPoint;
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"segment-{segmentId}";
            Bounds = _path.TightBounds;
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public string SegmentId { get; }

        public override void Render(SKCanvas canvas)
        {
            var currentPaint = SkiaPaints.SegmentPathPaint;

            if (IsSpawnPoint && !IsOnRoute) // Also check if it's on a route because it can't be both
            {
                currentPaint = SkiaPaints.SpawnPointSegmentPathPaint;
            }

            if (IsOnRoute)
            {
                currentPaint = SkiaPaints.RoutePathPaint;
            }

            if (IsLeadIn || IsLeadOut)
            {
                currentPaint = SkiaPaints.LeadInPaint;
            }

            if (IsSelected)
            {
                currentPaint = SkiaPaints.SelectedSegmentPathPaint;
            }

            if (IsHighlighted)
            {
                currentPaint = SkiaPaints.SegmentHighlightPaint;
            }


            canvas.DrawPath(_path, currentPaint);
        }
    }
}