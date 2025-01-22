// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class MarkerSegment : MapObject
    {
        private readonly SKPath _path;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _fillPaint;
        private readonly SKPaint _circlePaint;
        private readonly SKFont _font;
        private readonly float _offsetX;
        private readonly float _offsetY;
        private readonly SKPaint _squarePaint;
        private readonly SKPaint _squarePaintAlternate;
        private readonly string _startMarkerText;
        private readonly SKPaint _pathPaint;

        public MarkerSegment(string id, SKPoint[] points, string color, string startMarkerText, SKPaint pathPaint)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);

            Id = id;
            _startMarkerText = startMarkerText;
            _pathPaint = pathPaint;
            Name = id;
            Bounds = _path.TightBounds;

            _textPaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _fillPaint = new SKPaint { Color = SKColor.Parse(color), IsAntialias = true, Style = SKPaintStyle.Fill };
            _circlePaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            
            _squarePaint = new SKPaint { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Fill };
            _squarePaintAlternate = new SKPaint { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Fill };

            _font = new SKFont { Size = 16, Embolden = true };
            var glyphs = _textPaint.GetGlyphs(_startMarkerText);
            _font.MeasureText(glyphs, out var textBounds);
            _offsetX = textBounds.Width / 2;
            _offsetY = textBounds.Height / 2;
        }

        public string Id { get;}
        public override string Name { get; }
        public override SKRect Bounds { get; }
        public override bool IsSelectable { get; set; } = false;
        public override bool IsVisible { get; set; } = false;
        
        protected override void RenderCore(SKCanvas canvas)
        {
            if (IsVisible)
            {
                canvas.DrawPath(_path, _pathPaint);

                var radius = 12;
                var startPoint = _path.Points[0];
                canvas.DrawCircle(startPoint, radius, _fillPaint);
                canvas.DrawCircle(startPoint, radius, _circlePaint);

                canvas.DrawText(_startMarkerText, startPoint.X - _offsetX, startPoint.Y + _offsetY, _font, _textPaint);

                var finishFlagWidth = 18f;
                DrawFinishFlag(canvas, _path.Points[^1].X - finishFlagWidth / 2, _path.Points[^1].Y - finishFlagWidth / 2, finishFlagWidth);
            }
        }

        private void DrawFinishFlag(SKCanvas canvas, float x, float y, float width)
        {
            var numberOfSquares = 5;
            var squareSize = width / numberOfSquares;

            var boundsMidX = width / 2;
            var boundsMidY = boundsMidX;

            canvas.DrawCircle(x + boundsMidX, y + boundsMidY, boundsMidX + squareSize, _squarePaint);

            for (var row = 0; row < numberOfSquares; row++)
            {
                for (var index = 0; index < numberOfSquares; index++)
                {
                    canvas.DrawRect(
                        x + index * squareSize, 
                        y + row * squareSize, 
                        squareSize, 
                        squareSize,
                        index % 2 == row % 2 ? _squarePaint : _squarePaintAlternate);
                }
            }

            canvas.DrawCircle(x + boundsMidX, y + boundsMidY, boundsMidX + squareSize, _circlePaint);
        }
    }
}
