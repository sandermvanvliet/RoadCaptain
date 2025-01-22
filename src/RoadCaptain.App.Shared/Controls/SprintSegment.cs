// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class SprintSegment : MarkerSegment
    {
        public SprintSegment(string id, SKPoint[] points)
            : base($"sprint-{id}", points, "#56A91D", "S", SkiaPaints.SprintSegmentPaint)
        {
        }
    }
}
