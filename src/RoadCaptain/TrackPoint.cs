// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace RoadCaptain
{
    // TODO: Investigate converting to struct
    public class TrackPoint : IEquatable<TrackPoint>
    {
        private const double CoordinateEqualityTolerance = 0.00001d;
        private const double PiRad = Math.PI / 180d;
        private const double RadToDegree = 180 / Math.PI;
        private const double RadiusOfEarth = 6371;

        public TrackPoint(double latitude, double longitude, double altitude, ZwiftWorldId? worldId = null)
        {
            WorldId = worldId;
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
        public Segment? Segment { get; set; }
        
        [JsonIgnore]
        // RadiusOfEartheSharper disable once UnusedMember.Global because this is only used to look up a point using Garmin BaseCamp
        public string CoordinatesDecimal =>
            $"S{(Latitude * -1).ToString("0.00000", CultureInfo.InvariantCulture)}° E{Longitude.ToString("0.00000", CultureInfo.InvariantCulture)}°";

        public override string ToString()
        {
            return $"{Latitude.ToString("0.00000", CultureInfo.InvariantCulture)}, {Longitude.ToString("0.00000", CultureInfo.InvariantCulture)}, {Altitude.ToString("0.0", CultureInfo.InvariantCulture)}";
        }

        public TrackPoint Clone()
        {
            return new TrackPoint(Latitude, Longitude, Altitude)
            {
                Index = Index,
                Segment = Segment,
                DistanceFromLast = DistanceFromLast,
                DistanceOnSegment = DistanceOnSegment
            };
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
            var dLat = (lat2 - lat1) * PiRad;
            var dLon = (lon2 - lon1) * PiRad;

            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(lat1 * PiRad) * Math.Cos(lat2 * PiRad) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = RadiusOfEarth * c; // Distance in km

            return d * 1000;
        }
        
        public TrackPoint ProjectTo(double bearingInDegrees, double distanceInMeters, int? altitude = null)
        {
            /*
def get_point_at_distance(lat1, lon1, d, bearing, R=6371):
    """
    lat: initial latitude, in degrees
    lon: initial longitude, in degrees
    d: target distance from initial
    bearing: (true) heading in degrees
    R: optional radius of sphere, defaults to mean radius of earth

    Returns new lat/lon coordinate {d}km from initial, in degrees
    """
    lat1 = radians(lat1)
    lon1 = radians(lon1)
    a = radians(bearing)
    lat2 = asin(sin(lat1) * cos(d/R) + cos(lat1) * sin(d/R) * cos(a))
    lon2 = lon1 + atan2(
        sin(a) * sin(d/R) * cos(lat1),
        cos(d/R) - sin(lat1) * sin(lat2)
    )
    return (degrees(lat2), degrees(lon2),)
    */

            var latRadians = DegreesToRadians(Latitude);
            var lonRadians = DegreesToRadians(Longitude);
            var bearingInRadians = DegreesToRadians(bearingInDegrees);

            var newLatRadians = Math.Asin(Math.Sin(latRadians) * Math.Cos(distanceInMeters / RadiusOfEarth) +
                                          Math.Cos(latRadians) * Math.Sin(distanceInMeters / RadiusOfEarth) *
                                          Math.Cos(bearingInRadians));

            var newLonRadians = lonRadians + Math.Atan2(
                Math.Sin(bearingInRadians) * Math.Sin(distanceInMeters / RadiusOfEarth) * Math.Cos(latRadians),
                Math.Cos(distanceInMeters / RadiusOfEarth) - Math.Sin(latRadians) * Math.Sin(newLatRadians));

            return new TrackPoint(
                RadToDegree * newLatRadians,
                RadToDegree * newLonRadians,
                altitude ?? Altitude,
                WorldId);
        }

        public static double Bearing(TrackPoint pt1, TrackPoint pt2)
        {
            var x = Math.Cos(DegreesToRadians(pt1.Latitude)) * Math.Sin(DegreesToRadians(pt2.Latitude)) - Math.Sin(DegreesToRadians(pt1.Latitude)) * Math.Cos(DegreesToRadians(pt2.Latitude)) * Math.Cos(DegreesToRadians(pt2.Longitude - pt1.Longitude));
            var y = Math.Sin(DegreesToRadians(pt2.Longitude - pt1.Longitude)) * Math.Cos(DegreesToRadians(pt2.Latitude));

            // Math.Atan2 can return negative value, 0 <= output value < 2*PI expected 
            return (Math.Atan2(y, x) + Math.PI * 2) % (Math.PI * 2) * RadToDegree;
        }

        public static double DegreesToRadians(double angle)
        {
            return angle * PiRad;
        }

        public bool Equals(TrackPoint? other)
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

        public static bool Equals(TrackPoint? a, TrackPoint? b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public override bool Equals(object? obj)
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

        private static readonly ZwiftWorldConstants Watopia = new(110614.71d, 109287.52d, -11.644904f, 166.95293);
        private static readonly ZwiftWorldConstants Richmond = new(110987.82d, 88374.68d, 37.543f, -77.4374f);
        private static readonly ZwiftWorldConstants London = new(111258.3d, 69400.28d, 51.501705f, -0.16794094f);
        private static readonly ZwiftWorldConstants NewYork = new(110850.0d, 84471.0d, 40.76723f, -73.97667f);
        private static readonly ZwiftWorldConstants Innsbruck = new(111230.0d, 75027.0d, 47.2728f, 11.39574f);
        private static readonly ZwiftWorldConstants Bologna = new(111230.0d, 79341.0d, 44.49477f, 11.34324f);
        private static readonly ZwiftWorldConstants Yorkshire = new(111230.0d, 65393.0d, 53.991127f, -1.541751f);
        private static readonly ZwiftWorldConstants CritCity = new(110614.71d, 109287.52d, -10.3844f, 165.8011f);
        private static readonly ZwiftWorldConstants MakuriIslands = new(110614.71d, 109287.52d, -10.749806f, 165.83644f);
        private static readonly ZwiftWorldConstants France = new(110726.0d, 103481.0d, -21.695074f, 166.19745f);
        private static readonly ZwiftWorldConstants Paris = new(111230.0d, 73167.0, 48.86763f, 2.31413f);

        internal static readonly Dictionary<ZwiftWorldId, ZwiftWorldConstants> ZwiftWorlds =
            new()
            {
                { ZwiftWorldId.Watopia, Watopia },
                { ZwiftWorldId.Richmond, Richmond },
                { ZwiftWorldId.London, London },
                { ZwiftWorldId.NewYork, NewYork },
                { ZwiftWorldId.Innsbruck, Innsbruck },
                { ZwiftWorldId.Bologna, Bologna },
                { ZwiftWorldId.Yorkshire, Yorkshire },
                { ZwiftWorldId.CritCity, CritCity },
                { ZwiftWorldId.MakuriIslands, MakuriIslands },
                { ZwiftWorldId.France, France },
                { ZwiftWorldId.Paris, Paris },
            };

        public MapCoordinate ToMapCoordinate()
        {
            var worldId = WorldId.GetValueOrDefault(ZwiftWorldId.Watopia);
            ZwiftWorldConstants worldConstants;

            switch (worldId)
            {
                case ZwiftWorldId.Watopia:
                    worldConstants = Watopia;
                    break;
                case ZwiftWorldId.MakuriIslands:
                    worldConstants = MakuriIslands;
                    break;
                case ZwiftWorldId.Richmond:
                    worldConstants = Richmond;
                    break;
                case ZwiftWorldId.London:
                    worldConstants = London;
                    break;
                case ZwiftWorldId.NewYork:
                    worldConstants = NewYork;
                    break;
                case ZwiftWorldId.Innsbruck:
                    worldConstants = Innsbruck;
                    break;
                case ZwiftWorldId.Bologna:
                    worldConstants = Bologna;
                    break;
                case ZwiftWorldId.Yorkshire:
                    worldConstants = Yorkshire;
                    break;
                case ZwiftWorldId.CritCity:
                    worldConstants = CritCity;
                    break;
                case ZwiftWorldId.France:
                    worldConstants = France;
                    break;
                case ZwiftWorldId.Paris:
                    worldConstants = Paris;
                    break;
                default:
                    return MapCoordinate.Unknown;
            }

            // NOTE: The coordinates in Zwift itself are flipped which
            //       is why you see longitude used to calculate latitude
            //       and negative latitude to calculate longitude.
            var latitudeAsCentimetersFromOrigin = (Longitude * worldConstants.MetersBetweenLatitudeDegree * 100);
            var latitudeOffsetCentimeters = latitudeAsCentimetersFromOrigin - worldConstants.CenterLatitudeFromOrigin;

            var longitudeAsCentimetersFromOrigin = -Latitude * worldConstants.MetersBetweenLongitudeDegree * 100;
            var longitudeOffsetCentimeters = longitudeAsCentimetersFromOrigin - worldConstants.CenterLongitudeFromOrigin;

            return new MapCoordinate(latitudeOffsetCentimeters, longitudeOffsetCentimeters, Altitude, worldId);
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

        public static TrackPoint Unknown => new(Double.NaN, Double.NaN, Double.NaN);

        public ZwiftWorldId? WorldId { get; }

        public PositionDelta DeltaTo(TrackPoint other)
        {
            var distanceFromLast = DistanceTo(other);

            return new PositionDelta
            {
                Distance = (distanceFromLast / 1000),
                Ascent = Altitude < other.Altitude ? other.Altitude - Altitude : 0,
                Descent = Altitude < other.Altitude ? 0 : Altitude - other.Altitude
            };
        }
    }

    public struct PositionDelta
    {
        public double Distance { get; set; }
        public double Ascent { get; set; }
        public double Descent { get; set; }
    }
}
