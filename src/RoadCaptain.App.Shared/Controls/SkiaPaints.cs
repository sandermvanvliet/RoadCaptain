// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
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
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.Stroke, StrokeWidth = 8, IsAntialias = true };

        public static readonly SKPaint LeadInPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 4, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4) ,IsAntialias = true };
        
        public static readonly SKPaint LoopPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 ,IsAntialias = true };

        public static readonly SKPaint RiderPositionFillPaint = new()
            { Color = SKColor.Parse("#FF6141"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint CircleMarkerPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true };

        public static readonly SKPaint StartMarkerFillPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint EndMarkerFillPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill, IsAntialias = true };

        public static readonly SKPaint SprintSegmentPaint = new()
            { Color = SKColor.Parse("#B200FF"), Style = SKPaintStyle.Stroke, StrokeWidth = 6, IsAntialias = true };

        public static readonly SKPaint ClimbSegmentPaint = new()
            { Color = SKColor.Parse("#FF6A00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6, IsAntialias = true };

        // Elevation profile colors generated using ColorBrewer2: https://colorbrewer2.org/#type=sequential&scheme=YlOrRd&n=5
        public static readonly SKPaint ElevationProfileGradeZeroPaint = new() { Color = SKColor.Parse("#ffffb2"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true }; // blue
        public static readonly SKPaint ElevationProfileGradeThreePaint = new() { Color = SKColor.Parse("#fecc5c"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true }; // green
        public static readonly SKPaint ElevationProfileGradeFivePaint = new() { Color = SKColor.Parse("#fd8d3c"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true }; // yellow
        public static readonly SKPaint ElevationProfileGradeEightPaint = new() { Color = SKColor.Parse("#f03b20"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true }; // orange
        public static readonly SKPaint ElevationProfileGradeTenPaint = new() { Color = SKColor.Parse("#bd0026"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 2, IsAntialias = true }; // red

        public static readonly SKPaint ElevationLinePaint = new()
            { Color = SKColor.Parse("#0000ff").WithAlpha((byte)(0xFF * 0.75)), Style = SKPaintStyle.Stroke, StrokeWidth = 1, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 4f}, 1),IsAntialias = true };

        public static readonly SKPaint ElevationLineTextPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
    }
}
