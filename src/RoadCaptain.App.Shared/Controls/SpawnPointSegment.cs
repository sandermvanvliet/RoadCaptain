using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class SpawnPointSegment : MapObject
    {
        private readonly SKPath _path;
        private readonly float _angleRadians;
        private readonly SKPaint _arrowPaint;
        private readonly SKPaint _circlePaint;
        private readonly SKPaint _circleFillPaint;
        private const int CircleMarkerRadius = 16;

        public SpawnPointSegment(string segmentId, SKPoint[] points, double middleBearing)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"spawnPoint-{segmentId}";
            Bounds = _path.TightBounds;
            
            _angleRadians = (float)TrackPoint.DegreesToRadians(ConvertToSkiaAngle(middleBearing)); 
            _arrowPaint = new SKPaint{ Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Stroke, StrokeWidth = 6, IsAntialias = true};
            _circlePaint = new() { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
            _circleFillPaint = new() { Color = SKColor.Parse("#0094FF"), Style = SKPaintStyle.Fill, IsAntialias = true };
        }

        private static double ConvertToSkiaAngle(double middleBearing)
        {
            // middleBearing is compass bearing with 0 pointing up, Skia uses 0 as right of the origin.
            // That means that we need to turn right 90 degrees from compass to Skia angles...
            // But! Skia rotates the other way around so we need to correct for that which is why
            // we're _adding_ 90 degrees instead of subtracting it
            return middleBearing + 90;
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public string SegmentId { get; }
        public bool IsVisible { get; set; } = true;

        public override void Render(SKCanvas canvas)
        {
            if (IsVisible)
            {
                canvas.DrawPath(_path, SkiaPaints.SpawnPointSegmentPathPaint);
                
                var pathPoint = _path.Points[_path.PointCount/ 2];
                
                canvas.DrawCircle(pathPoint, CircleMarkerRadius, _circleFillPaint);
                canvas.DrawCircle(pathPoint, CircleMarkerRadius, _circlePaint);

                var matrix = SKMatrix.CreateRotation(_angleRadians);

                var size = CircleMarkerRadius - 4;

                var chevronPoints = matrix.MapPoints(new[]
                {
                    new SKPoint(size, size),
                    new SKPoint(0, 0),
                    new SKPoint(size, -size)
                });
                var stemPoints = matrix.MapPoints(new[]
                {
                    new SKPoint(0, 0),
                    new SKPoint(2 * size, 0)
                });
                
                var arrowPath = new SKPath();
                arrowPath.AddPoly(chevronPoints, false);
                arrowPath.AddPoly(stemPoints, false);
                
                arrowPath.Transform(SKMatrix.CreateTranslation(pathPoint.X - arrowPath.TightBounds.MidX, pathPoint.Y - arrowPath.TightBounds.MidY));

                canvas.DrawPath(arrowPath, _arrowPaint);
            }
        }
    }
}