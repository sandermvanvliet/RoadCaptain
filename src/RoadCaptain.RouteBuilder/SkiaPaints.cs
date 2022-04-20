using SkiaSharp;

namespace RoadCaptain.RouteBuilder
{
    public class SkiaPaints
    {
        public static readonly SKPaint _segmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        public static readonly SKPaint _selectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        public static readonly SKPaint _segmentHighlightPaint = new()
            { Color = SKColor.Parse("#4CFF00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        public static readonly SKPaint _spawnPointSegmentPathPaint = new()
            { Color = SKColor.Parse("#44dd44"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        public static readonly SKPaint _routePathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 8 };

        public static readonly SKPaint _riderPositionPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        public static readonly SKPaint _riderPositionFillPaint = new()
            { Color = SKColor.Parse("#FF6141"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint _startMarkerPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        public static readonly SKPaint _startMarkerFillPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint _endMarkerFillPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint _markerSegmentStartPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint _markerSegmentEndPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill };
    }
}