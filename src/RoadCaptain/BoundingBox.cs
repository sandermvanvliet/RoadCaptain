using System.Collections.Generic;

namespace RoadCaptain
{
    public class BoundingBox
    {
        public double MinLatitude { get; }
        public double MaxLatitude { get; }

        public double MinLongitude { get; }
        public double MaxLongitude { get; }
        
        // Make the bounding box larger than the points on the segment.
        // That ensures that we can still match game positions properly
        // as they are never exactly aligned with the segment positions.
        private const double LatitudeMargin = 0.01d;
        private const double LongitudeMargin = 0.01d;

        public BoundingBox(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
        {
            MinLongitude = minLongitude - LongitudeMargin;
            MinLatitude = minLatitude - LatitudeMargin;
            MaxLongitude = maxLongitude + LongitudeMargin;
            MaxLatitude = maxLatitude + LatitudeMargin;
        }

        public bool IsIn(TrackPoint point)
        {
            return point.Latitude >= MinLatitude &&
                   point.Latitude <= MaxLatitude &&
                   point.Longitude >= MinLongitude &&
                   point.Longitude <= MaxLongitude;
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