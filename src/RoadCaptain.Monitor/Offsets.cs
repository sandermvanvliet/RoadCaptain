using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.Monitor
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
    }
}