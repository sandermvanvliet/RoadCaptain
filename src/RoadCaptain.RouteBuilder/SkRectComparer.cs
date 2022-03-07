using System.Collections.Generic;
using SkiaSharp;

namespace RoadCaptain.RouteBuilder
{
    internal class SkRectComparer : IComparer<SKRect>
    {
        public int Compare(SKRect x, SKRect y)
        {
            var areaX = x.Width * x.Height;
            var areaY = y.Width * y.Height;

            return areaX.CompareTo(areaY);
        }
    }
}