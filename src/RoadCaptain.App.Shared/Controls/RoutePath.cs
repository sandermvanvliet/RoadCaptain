// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
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

        protected override void RenderCore(SKCanvas canvas)
        {
            if (!_path.Points.Any())
            {
                return;
            }

            if (ShowFullPath)
            {
                canvas.DrawPath(_path, SkiaPaints.RoutePathPaint);
            }
            
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

        public override bool IsVisible { get; set; } = true;
        public override string Name { get; }
        public override SKRect Bounds { get; }
        public override bool IsSelectable { get; set; } = false;
        public bool ShowFullPath { get; set; }

        public SKPoint? Current
        {
            get
            {
                if (_currentPosition == -1)
                {
                    return null;
                }

                if (_currentPosition >= _path.Points.Length)
                {
                    _currentPosition = 0;
                }

                return _path.Points[_currentPosition];
            }
        }

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
