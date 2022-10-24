using System.Linq;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public class RoutePath : MapObject
    {
        private readonly SKPath _path;
        private int _currentPosition = -1;
        private const int CircleMarkerRadius = 16;

        public RoutePath(SKPoint[] points)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);
            
            Name = "route";
            Bounds = _path.TightBounds;
        }
        
        public override void Render(SKCanvas canvas)
        {
            if (!_path.Points.Any() || !IsVisible)
            {
                return;
            }
            
            canvas.DrawPath(_path, SkiaPaints.RoutePathPaint);
            
            canvas.DrawCircle(_path.Points[0], CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
            canvas.DrawCircle(_path.Points[0], CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, SkiaPaints.StartMarkerFillPaint);
            
            canvas.DrawCircle(_path.Points[^1], CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
            canvas.DrawCircle(_path.Points[^1], CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, SkiaPaints.EndMarkerFillPaint);

            if (_currentPosition >= 0 && _currentPosition < _path.PointCount)
            {
                canvas.DrawCircle(_path.Points[_currentPosition], CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
                canvas.DrawCircle(_path.Points[_currentPosition], CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, SkiaPaints.RiderPositionFillPaint);
            }
        }

        public bool IsVisible { get; set; }
        public override string Name { get; }
        public override SKRect Bounds { get; }

        public void MoveNext()
        {
            if (_currentPosition + 1 > _path.PointCount)
            {
                Reset();
                return;
            }
            
            _currentPosition++;
            
        }

        public void Reset()
        {
            _currentPosition = -1;
        }
    }
}