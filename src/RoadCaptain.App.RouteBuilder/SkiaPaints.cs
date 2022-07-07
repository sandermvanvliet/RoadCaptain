using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder
{
    public class SkiaPaints
    {
        public static readonly SKPaint SegmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };

        public static readonly SKPaint SelectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6, IsAntialias = true };

        public static readonly SKPaint SegmentHighlightPaint = new()
            { Color = SKColor.Parse("#4CFF00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6, IsAntialias = true };

        public static readonly SKPaint SpawnPointSegmentPathPaint = new()
            { Color = SKColor.Parse("#44dd44"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };

        public static readonly SKPaint RoutePathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };

        public static readonly SKPaint RiderPositionFillPaint = new()
            { Color = SKColor.Parse("#FF6141"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint CircleMarkerPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true };

        public static readonly SKPaint StartMarkerFillPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint EndMarkerFillPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint MarkerSegmentStartPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint MarkerSegmentEndPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint SprintSegmentPaint = new()
            { Color = SKColor.Parse("#B200FF"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };

        public static readonly SKPaint ClimbSegmentPaint = new()
            { Color = SKColor.Parse("#FF6A00"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = true };

        public static readonly SKPaint ElevationPlotClimbSegmentPaint = new()
            { Color = SKColor.Parse("#FF6A00"), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };

        public static readonly SKPaint ElevationPlotClimbTextPaint = new()
            { Color = SKColor.Parse("#FF6A00"), Style = SKPaintStyle.Fill, IsAntialias = true };
        
        public static readonly SKPaint ElevationPlotPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
        
        public static readonly SKPaint ElevationPlotBackgroundPaint = new()
            { Color = new SKColor(204, 204, 204, 100), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint ElevationLinePaint = new()
            { Color = SKColor.Parse("#0000ff").WithAlpha((byte)(0xFF * 0.75)), Style = SKPaintStyle.Stroke, StrokeWidth = 1, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 4f}, 1),IsAntialias = true };

        public static readonly SKPaint ElevationLineTextPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        
    }
}