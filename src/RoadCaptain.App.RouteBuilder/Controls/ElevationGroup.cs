using System.Collections.Generic;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    internal class ElevationGroup
    {
        public void Add(TrackPoint trackPoint)
        {
            Points.Add(trackPoint);
        }

        public List<TrackPoint> Points { get; } = new();
        public double Grade { get; set; }
        public SKPath? Path { get; set; }
    }
}