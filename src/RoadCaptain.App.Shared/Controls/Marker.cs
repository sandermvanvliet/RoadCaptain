// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    internal class Marker
    {
        public string Id { get; set; }
        public SKPoint StartDrawPoint { get; set; }
        public SKPoint EndDrawPoint { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public string Name { get; set; }
        public SKPath Path { get; set; }
        public SegmentType Type { get; set; }
        public TrackPoint StartPoint { get; set; }
        public TrackPoint EndPoint { get; set; }
        public SKRect Bounds { get; set; }
    }
}
