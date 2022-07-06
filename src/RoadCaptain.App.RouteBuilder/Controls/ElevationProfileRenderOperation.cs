using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ElevationProfileRenderOperation : ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private SKBitmap? _bitmap;
        private RouteViewModel? _route;
        private SKPath? _elevationPath;
        private Rect _bounds;

        public RouteViewModel? Route
        {
            get => _route;
            set
            {
                _route = value;
                CreateElevationProfile();
            }
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                InitializeBitmap();
                CreateElevationProfile();
            }
        }

        public List<Segment?>? Segments { get; set; }

        public void Dispose()
        {
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

            if (canvas == null)
            {
                return;
            }

            canvas.Clear(CanvasBackgroundColor);

            if (_bitmap is { Width: > 0 })
            {
                // TODO: Something smart so that we only render when actually needed
                using (var mapCanvas = new SKCanvas(_bitmap))
                {
                    RenderCanvas(mapCanvas);
                }

                canvas.DrawBitmap(_bitmap, 0, 0);
            }
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is ElevationProfileRenderOperation)
            {
                return true;
            }

            return false;
        }

        private void CreateElevationProfile()
        {
            if (Route == null || Segments == null || !Segments.Any() || !Route.Sequence.Any())
            {
                _elevationPath = null;

                return;
            }
            
            var routePoints = new List<TrackPoint>();

            foreach (var routeStep in Route.Sequence)
            {
                var segment = GetSegmentById(routeStep.SegmentId);
                
                if (segment == null)
                {
                    // TODO: yolo!
                    continue;
                }

                var points = segment.Points.ToArray();

                if (routeStep.Direction == SegmentDirection.BtoA)
                {
                    points = points.Reverse().ToArray();
                }

                routePoints.AddRange(points);
            }

            var maxAltitude = routePoints.Max(point => point.Altitude);
            var altitudeScaleFactor = (Bounds.Height - 10) / maxAltitude;

            var step = 1f;

            if (routePoints.Count > Bounds.Width)
            {
                step = (float)(Bounds.Width / routePoints.Count);
            }

            var polyPoints = routePoints
                .Select((point, index) => new SKPoint(step * index, (float)(point.Altitude * altitudeScaleFactor)))
                .ToArray();

            _elevationPath = new SKPath();
            _elevationPath = new SKPath();
            _elevationPath.AddPoly(polyPoints, false);
        }

        private Segment? GetSegmentById(string id)
        {
            return Segments?.SingleOrDefault(s => s.Id == id);
        }

        private void RenderCanvas(SKCanvas canvas)
        {
            canvas.Clear(CanvasBackgroundColor);

            if (_elevationPath == null || _elevationPath.PointCount == 0)
            {
                return;
            }

            canvas.Scale(1, -1);
            canvas.Translate(0, -(float)Bounds.Height);

            var lastPoint = _elevationPath.Points.Last();
            var backgroundPath =new SKPath();
            backgroundPath.AddPoly(
                _elevationPath.Points.Concat(new[] { new SKPoint(lastPoint.X, 0), new SKPoint(0, 0) }).ToArray());

            canvas.DrawPath(backgroundPath, SkiaPaints.ElevationPlotBackgroundPaint);

            canvas.DrawPath(_elevationPath, SkiaPaints.ElevationPlotPaint);
        }

        private void InitializeBitmap()
        {
            _bitmap = new SKBitmap((int)Bounds.Width, (int)Bounds.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            
            using var canvas = new SKCanvas(_bitmap);
        }
    }
}