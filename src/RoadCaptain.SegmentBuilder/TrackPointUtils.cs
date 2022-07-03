using System;

namespace RoadCaptain.SegmentBuilder
{
    static internal class TrackPointUtils
    {
        public static bool IsCloseTo(TrackPoint point, TrackPoint other, int radiusMeters = 15)
        {
            // 0.00013 degrees equivalent to 15 meters between degrees at latitude -11 
            // That means that if the difference in longitude between
            // the two points is more than 0.00013 then we're definitely
            // going to be more than 15 meters apart and that means
            // we're not close.
            if (Math.Abs(other.Longitude - point.Longitude) > 0.00013)
            {
                return false;
            }

            var distance = TrackPoint.GetDistanceFromLatLonInMeters(
                other.Latitude,
                other.Longitude,
                point.Latitude,
                point.Longitude);
            
            if (distance < radiusMeters && Math.Abs(other.Altitude - point.Altitude) <= 2d)
            {
                return true;
            }

            return false;
        }
    }
}