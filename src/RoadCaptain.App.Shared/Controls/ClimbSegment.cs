// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class ClimbSegment : MarkerSegment
    {
        public ClimbSegment(string id, SKPoint[] points)
            : base($"climb-{id}", points, "#fc4119", "K", SkiaPaints.ClimbSegmentPaint)
        {
        }
    }
}
