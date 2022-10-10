// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.SegmentBuilder
{
    internal static class TrackPointUtils
    {
        public static bool IsCloseTo(TrackPoint point, TrackPoint other, int radiusMeters = 15)
        {
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
