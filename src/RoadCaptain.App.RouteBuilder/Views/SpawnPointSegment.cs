using System.Diagnostics;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public class SpawnPointSegment : MapObject
    {
        private readonly SKPath _path;

        public SpawnPointSegment(string segmentId, SKPoint[] points)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"spawnPoint-{segmentId}";
            Bounds = _path.TightBounds;
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public string SegmentId { get; }
        public bool IsVisible { get; set; } = true;

        public override void Render(SKCanvas canvas)
        {
            if (IsVisible)
            {
                //Debug.WriteLine($"{SegmentId} is a spawn-point");
                canvas.DrawPath(_path, SkiaPaints.SpawnPointSegmentPathPaint);
                
                // TODO: draw direction arrow
            }
        }
    }
}