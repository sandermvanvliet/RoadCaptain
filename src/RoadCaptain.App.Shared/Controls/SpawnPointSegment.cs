// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class SpawnPointSegment : MapObject
    {
        private readonly SKPath _path;
        private readonly SKPaint _arrowPaint;
        private readonly SKPaint _circlePaint;
        private readonly SKPaint _circleFillPaint;
        private readonly SKPath? _arrowPath;
        private readonly SKPoint _centerPoint;
        private const int CircleMarkerRadius = 16;

        public SpawnPointSegment(string segmentId, SKPoint[] points, double middleBearing, bool isTwoWay)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"spawnPoint-{segmentId}";
            Bounds = _path.TightBounds;

            _arrowPaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };
            _circlePaint = new() { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Stroke, StrokeWidth = 3, IsAntialias = true };
            _circleFillPaint = new() { Color = SKColor.Parse("#0094FF"), Style = SKPaintStyle.Fill, IsAntialias = true };

            if (points.Any())
            {
                _centerPoint = _path.Points[_path.PointCount / 2];

                var angleRadians = (float)TrackPoint.DegreesToRadians(ConvertToSkiaAngle(middleBearing));

                _arrowPath = isTwoWay
                    ? CreateTwoWayArrowPath(angleRadians)
                    : CreateArrowPath(angleRadians);
            }
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

        public override bool IsSelectable
        {
            get => true;
            set {}
        }
        public string SegmentId { get; }
        public override bool IsVisible { get; set; } = true;

        protected override void RenderCore(SKCanvas canvas)
        {
            if (IsVisible)
            {
                canvas.DrawPath(_path, SkiaPaints.SpawnPointSegmentPathPaint);

                canvas.DrawCircle(_centerPoint, CircleMarkerRadius, _circleFillPaint);
                canvas.DrawCircle(_centerPoint, CircleMarkerRadius, _circlePaint);

                canvas.DrawPath(_arrowPath, _arrowPaint);
            }
        }

        private SKPath CreateArrowPath(float angleRadians)
        {
            const int size = CircleMarkerRadius - 4;

            var matrix = SKMatrix.CreateRotation(angleRadians);

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

            arrowPath
                .Transform(
                    SKMatrix.CreateTranslation(
                        _centerPoint.X - arrowPath.TightBounds.MidX,
                        _centerPoint.Y - arrowPath.TightBounds.MidY));

            return arrowPath;
        }

        private SKPath CreateTwoWayArrowPath(float angleRadians)
        {
            const int size = CircleMarkerRadius - 4;

            var matrix = SKMatrix.CreateRotation(angleRadians);

            var leftChevronParts = matrix.MapPoints(new[]
            {
                new SKPoint(size - 2, size - 2),
                new SKPoint(0, 0),
                new SKPoint(size - 2, -(size - 2))
            });

            var rightChevronParts = matrix.MapPoints(new[]
            {
                new SKPoint(size + 2, size - 2),
                new SKPoint(2 * size, 0),
                new SKPoint(size + 2, -(size - 2))
            });

            var stemPoints = matrix.MapPoints(new[]
            {
                new SKPoint(0, 0),
                new SKPoint(2 * size, 0)
            });

            var arrowPath = new SKPath();

            arrowPath.AddPoly(leftChevronParts, false);
            arrowPath.AddPoly(stemPoints, false);
            arrowPath.AddPoly(rightChevronParts, false);

            arrowPath
                .Transform(
                    SKMatrix.CreateTranslation(
                        _centerPoint.X - arrowPath.TightBounds.MidX,
                        _centerPoint.Y - arrowPath.TightBounds.MidY));

            return arrowPath;
        }
    }
}
