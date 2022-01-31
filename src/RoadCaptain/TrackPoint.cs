using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace RoadCaptain
{
    public class TrackPoint : IEquatable<TrackPoint>
    {
        private static readonly double PiRad = Math.PI / 180d;

        public TrackPoint(decimal latitude, decimal longitude, decimal altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public decimal Altitude { get; }
        public int Index { get; set; }
        public decimal DistanceOnSegment { get; set; }
        public decimal DistanceFromLast { get; set; }
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
            var distance = GetDistanceFromLatLonInMeters(
                (double)this.Latitude, (double)this.Longitude,
                (double)point.Latitude, (double)point.Longitude);

            if (distance < 15 && Math.Abs(this.Altitude - point.Altitude) <= 2m)
            {
                return true;
            }

            return false;
        }

        public static decimal GetDistanceFromLatLonInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double radiusOfEarth = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1); // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = radiusOfEarth * c; // Distance in km

            return (decimal)d * 1000;
        }

        private static double Deg2Rad(double deg)
        {
            return deg * PiRad;
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

            return Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude;
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((TrackPoint)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Altitude, Segment);
        }
    }
}