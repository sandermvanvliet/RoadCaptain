// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RoadCaptain.App.RouteBuilder.Models
{
    public class Offsets
    {
        private readonly int _offset;

        public Offsets(float imageWidth, float imageHeight, List<TrackPoint> data)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;

            MinX = (float)data.Min(p => p.Latitude);
            MaxX = (float)data.Max(p => p.Latitude);
                   
            MinY = (float)data.Min(p => p.Longitude);
            MaxY = (float)data.Max(p => p.Longitude);
        }

        private Offsets(float minX, float maxX, float minY, float maxY, float imageWidth, float imageHeight, int offset = 0)
        {
            _offset = offset;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        public float ImageWidth { get; }
        public float ImageHeight { get; }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinY { get; }
        public float MaxY { get; }
        public float RangeX => MaxX - MinX;
        public float RangeY => MaxY - MinY;

        // If minX is negative the offset is positive because we shift everything to the right, if it is positive the offset is negative because we shift to the left
        public float OffsetX => -MinX;
        public float OffsetY => -MinY;

        public float ScaleFactor
        {
            get
            {
                if (RangeY > RangeX)
                {
                    return (ImageHeight - 1) / RangeY;
                }

                return (ImageWidth - 1) / RangeX;
            }
        }

        public PointF ScaleAndTranslate(TrackPoint point)
        {
            return new PointF(
                _offset + (OffsetX + (float)point.Latitude) * ScaleFactor, 
                _offset + (OffsetY + (float)point.Longitude) * ScaleFactor);
        }

        public static Offsets From(List<Offsets> offsets)
        {
            var minX = offsets.Min(o => o.MinX);
            var maxX = offsets.Max(o => o.MaxX);
            var minY = offsets.Min(o => o.MinY);
            var maxY = offsets.Max(o => o.MaxY);

            return new Offsets(minX, maxX, minY, maxY, offsets.First().ImageWidth, offsets.First().ImageHeight);
        }

        public TrackPoint ReverseScaleAndTranslate(double x, double y)
        {
            return new TrackPoint(
                (x / ScaleFactor) - OffsetX - _offset,
                (y / ScaleFactor) - OffsetY - _offset,
                0);
        }

        public Offsets Pad(int offset)
        {
            var doubleOffset = 2 * offset;
            return new Offsets(MinX, MaxX, MinY, MaxY, ImageWidth - doubleOffset, ImageHeight - doubleOffset, offset);
        }
    }
}
