// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    internal class ElevationGroup
    {
        public void Add(TrackPoint trackPoint)
        {
            Points.Add(trackPoint);
        }

        public List<TrackPoint> Points { get; } = new();
        public int Grade { get; set; }
        public SKPath? Path { get; set; }
    }
}
