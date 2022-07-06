using System.Collections.Generic;
using System.Globalization;
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
        private float _altitudeOffset;
        private readonly int _padding = 10;
        private double _altitudeScaleFactor;
        private List<float> _elevationLines = new();

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

        public List<Segment>? Segments { get; set; }

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

            var minAltitude = routePoints.Min(point => point.Altitude);
            var maxAltitude = routePoints.Max(point => point.Altitude);

            // When min is above sea level use max as the delta, otherwise include the min
            var altitudeDelta = minAltitude < 0 ? -minAltitude + maxAltitude : maxAltitude;
            
            _altitudeScaleFactor = (Bounds.Height - (2 * _padding)) / altitudeDelta;
            
            // When min is below sea level we need to correct the resulting coordinate
            // so it isn't rendered off-screen.
            _altitudeOffset = (float)(minAltitude < 0 ? -minAltitude : 0);

            var step = routePoints.Count > Bounds.Width
                ? (float)(Bounds.Width / routePoints.Count)
                : 1f;

            var polyPoints = routePoints
                .Select((point, index) => new SKPoint(step * index, CalculateYFromAltitude(point.Altitude)))
                .ToArray();

            _elevationPath = new SKPath();
            _elevationPath.AddPoly(polyPoints, false);

            _elevationLines.Clear();
            _elevationLines.Add(0); // Always ensure sea-level exists

            if (altitudeDelta is > 50 and < 250)
            {
                var altitudeStep = 50;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
            else if (altitudeDelta is > 100 and < 1000)
            {
                var altitudeStep = 100;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
            else if (altitudeDelta is > 250 and < 5000)
            {
                var altitudeStep = 250;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
        }

        private float CalculateYFromAltitude(double altitude)
        {
            return (float)((altitude + _altitudeOffset) * _altitudeScaleFactor) + _padding;
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
            
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            canvas.Scale(1, -1);
            canvas.Translate(0, -(float)Bounds.Height);

            var lastPoint = _elevationPath.Points.Last();
            var backgroundPath =new SKPath();
            backgroundPath.AddPoly(
                _elevationPath.Points.Concat(new[] { new SKPoint(lastPoint.X, 0), new SKPoint(0, 0) }).ToArray());

            canvas.DrawPath(backgroundPath, SkiaPaints.ElevationPlotBackgroundPaint);

            canvas.DrawPath(_elevationPath, SkiaPaints.ElevationPlotPaint);
            
            // Back to normal
            canvas.Restore();

            var defaultFont = new SKFont(SKTypeface.Default);
            foreach (var elevation in _elevationLines)
            {
                var correctedAltitudeOffset = (float)(Bounds.Height - CalculateYFromAltitude(elevation));

                canvas.DrawLine(0, correctedAltitudeOffset, (float)Bounds.Width, correctedAltitudeOffset,
                    SkiaPaints.ElevationLinePaint);
                
                var text = elevation == 0 ? "Sea level" : elevation.ToString(CultureInfo.InvariantCulture) + "m";

                canvas.DrawText(text, 5, correctedAltitudeOffset, defaultFont, SkiaPaints.ElevationLineTextPaint);
            }
        }

        private void InitializeBitmap()
        {
            _bitmap = new SKBitmap((int)Bounds.Width, (int)Bounds.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            
            using var canvas = new SKCanvas(_bitmap);
        }
    }
}