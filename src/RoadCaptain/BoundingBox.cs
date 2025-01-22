// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;

namespace RoadCaptain
{
    public class BoundingBox
    {
        private readonly double _minLatitude;
        private readonly double _maxLatitude;
        private readonly double _minLongitude;
        private readonly double _maxLongitude;

        // Make the bounding box larger than the points on the segment.
        // That ensures that we can still match game positions properly
        // as they are never exactly aligned with the segment positions.
        private const double LatitudeMargin = 0.01d;
        private const double LongitudeMargin = 0.01d;

        public BoundingBox(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
        {
            _minLongitude = minLongitude - LongitudeMargin;
            _minLatitude = minLatitude - LatitudeMargin;
            _maxLongitude = maxLongitude + LongitudeMargin;
            _maxLatitude = maxLatitude + LatitudeMargin;
        }

        public bool IsIn(TrackPoint point)
        {
            return point.Latitude >= _minLatitude &&
                   point.Latitude <= _maxLatitude &&
                   point.Longitude >= _minLongitude &&
                   point.Longitude <= _maxLongitude;
        }

        public bool Contains(BoundingBox other)
        {
            return other._minLatitude >= _minLatitude &&
                   other._maxLatitude <= _maxLatitude &&
                   other._minLongitude >= _minLongitude &&
                   other._maxLongitude <= _maxLongitude;
        }

        public bool Overlaps(BoundingBox other)
        {
            return other._minLatitude < _maxLatitude && other._maxLatitude > _minLatitude &&
                   other._minLongitude < _maxLongitude && other._maxLongitude > _minLatitude;
        }

        public static BoundingBox From(List<TrackPoint> points)
        {
            double? minLongitude = null;
            double? minLatitude = null;
            double? maxLatitude = null;
            double? maxLongitude = null;

            foreach (var point in points)
            {
                if (!minLongitude.HasValue || point.Longitude < minLongitude.Value)
                {
                    minLongitude = point.Longitude;
                }
                
                if (!maxLongitude.HasValue || point.Longitude > maxLongitude.Value)
                {
                    maxLongitude = point.Longitude;
                }
                
                if (!minLatitude.HasValue || point.Latitude < minLatitude.Value)
                {
                    minLatitude = point.Latitude;
                }
                
                if (!maxLatitude.HasValue || point.Latitude > maxLatitude.Value)
                {
                    maxLatitude = point.Latitude;
                }
            }

            return new BoundingBox(
                minLongitude.GetValueOrDefault(0), 
                minLatitude.GetValueOrDefault(0), 
                maxLongitude.GetValueOrDefault(0),
                maxLatitude.GetValueOrDefault(0));
        }
    }
}
