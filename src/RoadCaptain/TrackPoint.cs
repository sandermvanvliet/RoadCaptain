// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace RoadCaptain
{
    public class TrackPoint : IEquatable<TrackPoint>
    {
        private const double CoordinateEqualityTolerance = 0.0001d;
        private const double PiRad = Math.PI / 180d;

        public TrackPoint(double latitude, double longitude, double altitude)
        {
            Latitude = Math.Round(latitude, 5);
            Longitude = Math.Round(longitude, 5);
            Altitude = altitude;
        }

        public double Latitude { get; }
        public double Longitude { get; }
        public double Altitude { get; }
        public int? Index { get; set; }
        public double DistanceOnSegment { get; set; }
        public double DistanceFromLast { get; set; }
        [JsonIgnore]
        public Segment Segment { get; set; }
        
        // ReSharper disable once UnusedMember.Global because this is only used to look up a point using Garmin BaseCamp
        public string CoordinatesDecimal =>
            $"S{(Latitude * -1).ToString("0.00000", CultureInfo.InvariantCulture)}° E{Longitude.ToString("0.00000", CultureInfo.InvariantCulture)}°";

        public override string ToString()
        {
            return $"{Latitude.ToString("0.00000", CultureInfo.InvariantCulture)}, {Longitude.ToString("0.00000", CultureInfo.InvariantCulture)}, {Altitude.ToString("0.0", CultureInfo.InvariantCulture)}";
        }

        public TrackPoint Clone()
        {
            return new TrackPoint(Latitude, Longitude, Altitude);
        }

        public bool IsCloseTo(TrackPoint point)
        {
            // 0.00013 degrees equivalent to 15 meters between degrees at latitude -11 
            // That means that if the difference in longitude between
            // the two points is more than 0.00013 then we're definitely
            // going to be more than 15 meters apart and that means
            // we're not close.
            if (Math.Abs(Longitude - point.Longitude) > 0.00013)
            {
                return false;
            }

            var distance = GetDistanceFromLatLonInMeters(
                Latitude, 
                Longitude,
                point.Latitude, 
                point.Longitude);

            // TODO: re-enable altitude matching
            if (distance < 15 /*&& Math.Abs(this.Altitude - point.Altitude) <= 2m*/)
            {
                return true;
            }

            return false;
        }

        public double DistanceTo(TrackPoint point)
        {
            return GetDistanceFromLatLonInMeters(
                Latitude, Longitude,
                point.Latitude, point.Longitude);
        }

        public static double GetDistanceFromLatLonInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double radiusOfEarth = 6371; // Radius of the earth in km
            var dLat = (lat2 - lat1) * PiRad;
            var dLon = (lon2 - lon1) * PiRad;

            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(lat1 * PiRad) * Math.Cos(lat2 * PiRad) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = radiusOfEarth * c; // Distance in km

            return d * 1000;
        }

        public bool Equals(TrackPoint other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Math.Abs(Latitude - other.Latitude) < CoordinateEqualityTolerance && 
                   Math.Abs(Longitude - other.Longitude) < CoordinateEqualityTolerance && 
                   Math.Abs(Altitude - other.Altitude) < CoordinateEqualityTolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((TrackPoint)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Altitude, Segment);
        }
        
        public static TrackPoint LatLongToGame(double latitude, double longitude, double altitude)
        {
            const double metersBetweenLatitudeDegree = 110614.71d;
            const double metersBetweenLongitudeDegree = 109287.52d;

            const double watopiaCenterLatitudeAsCentimetersFromOrigin = -128809767.893784d;
            const double watopiaCenterLongitudeAsCentimetersFromOrigin = 1824587167.6433601d;

            var latitudeAsCentimetersFromOrigin = (latitude * metersBetweenLatitudeDegree * 100);
            var latitudeOffsetCentimeters = latitudeAsCentimetersFromOrigin - watopiaCenterLatitudeAsCentimetersFromOrigin;

            var longitudeAsCentimetersFromOrigin = longitude * metersBetweenLongitudeDegree * 100;
            var longitudeOffsetCentimeters = longitudeAsCentimetersFromOrigin - watopiaCenterLongitudeAsCentimetersFromOrigin;

            return new TrackPoint(latitudeOffsetCentimeters, longitudeOffsetCentimeters, altitude);
        }

        public static TrackPoint FromGameLocation(double latitudeOffsetCentimeters, double longitudeOffsetCentimeters, double altitude)
        {
            const double metersBetweenLatitudeDegree = 110614.71d;
            const double metersBetweenLongitudeDegree = 109287.52d;

            const double watopiaCenterLatitudeAsCentimetersFromOrigin = -128809767.893784d;
            const double watopiaCenterLongitudeAsCentimetersFromOrigin = 1824587167.6433601d;

            var latitudeAsCentimetersFromOrigin = latitudeOffsetCentimeters + watopiaCenterLatitudeAsCentimetersFromOrigin;
            var latitude = latitudeAsCentimetersFromOrigin / metersBetweenLatitudeDegree / 100;

            var longitudeAsCentimetersFromOrigin = longitudeOffsetCentimeters + watopiaCenterLongitudeAsCentimetersFromOrigin;
            var longitude = longitudeAsCentimetersFromOrigin / metersBetweenLongitudeDegree / 100;

            return new TrackPoint(latitude, longitude, altitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCloseToQuick(double longitude, TrackPoint position)
        {
            // 0.00013 degrees equivalent to 15 meters between degrees at latitude -11 
            // That means that if the difference in longitude between
            // the two points is more than 0.00013 then we're definitely
            // going to be more than 15 meters apart and that means
            // we're not close.
            return Math.Abs(longitude - position.Longitude) < 0.00013;
        }
    }
}
