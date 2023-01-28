// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
