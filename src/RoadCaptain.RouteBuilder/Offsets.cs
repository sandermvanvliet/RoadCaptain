using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.RouteBuilder
{
    public class Offsets
    {
        public Offsets(float imageWidth, List<TrackPoint> data)
        {
            ImageWidth = imageWidth;

            MinX = (float)data.Min(p => p.Latitude);
            MaxX = (float)data.Max(p => p.Latitude);
                   
            MinY = (float)data.Min(p => p.Longitude);
            MaxY = (float)data.Max(p => p.Longitude);
        }

        private Offsets(float minX, float maxX, float minY, float maxY, float imageWidth)
        {
            ImageWidth = imageWidth;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        public float ImageWidth { get; }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinY { get; }
        public float MaxY { get; }
        public float RangeX => MaxX - MinX;
        public float RangeY => MaxY - MinY;

        // If minX is negative the offset is positive because we shift everything to the right, if it is positive the offset is negative beause we shift to the left
        public float OffsetX => -MinX;
        public float OffsetY => -MinY;

        public float ScaleFactor
        {
            get
            {
                if (RangeY > RangeX)
                {
                    return (ImageWidth - 1) / RangeY;
                }

                return (ImageWidth - 1) / RangeX;
            }
        }

        public static Offsets From(List<Offsets> offsets)
        {
            var minX = offsets.Min(o => o.MinX);
            var maxX = offsets.Max(o => o.MaxX);
            var minY = offsets.Min(o => o.MinY);
            var maxY = offsets.Max(o => o.MaxY);

            return new Offsets(minX, maxX, minY, maxY, offsets.First().ImageWidth);
        }
    }
}