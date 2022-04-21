using SkiaSharp;

namespace RoadCaptain.RouteBuilder
{
    public class SkiaPaints
    {
        public static readonly SKPaint SegmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        public static readonly SKPaint SelectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        public static readonly SKPaint SegmentHighlightPaint = new()
            { Color = SKColor.Parse("#4CFF00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        public static readonly SKPaint SpawnPointSegmentPathPaint = new()
            { Color = SKColor.Parse("#44dd44"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        public static readonly SKPaint RoutePathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 8 };

        public static readonly SKPaint RiderPositionPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        public static readonly SKPaint RiderPositionFillPaint = new()
            { Color = SKColor.Parse("#FF6141"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint StartMarkerPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        public static readonly SKPaint StartMarkerFillPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint EndMarkerFillPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint MarkerSegmentStartPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint MarkerSegmentEndPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill };

        public static readonly SKPaint SprintSegmentPaint = new()
            { Color = SKColor.Parse("#B200FF"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        public static readonly SKPaint ClimbSegmentPaint = new()
            { Color = SKColor.Parse("#FF6A00"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };
    }
}