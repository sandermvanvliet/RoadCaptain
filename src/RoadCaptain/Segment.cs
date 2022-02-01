using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain
{
    public class Segment
    {
        private const decimal LatitudeOffset = -562.03m;
        private const decimal LongitudeOffset = -562.03m;

        private static readonly Dictionary<int, decimal[]> LatitudeLongitudeOffsets = new()
        {
            { 1, new[] { -11.644904m, 166.95293m } },
            { 2, new[] { 37.543m, -77.4374m } },
            { 3, new[] { 51.501705m, -0.16794094m } },
            { 4, new[] { 40.76723m, -73.97667m } },
            { 5, new[] { 47.2728m, 11.39574m } },
            { 6, new[] { 44.49477m, 11.34324m } },
            { 7, new[] { 53.991127m, -1.541751m } },
            { 8, new[] { -10.3844m, 165.8011m } },
            { 9, new[] { -10.749806m, 165.83644m } },
            { 10, new[] { -21.695074m, 166.19745m } },
            { 11, new[] { 48.86763m, 2.31413m } }
        };

        private static readonly Dictionary<int, decimal[]> LatitudeLongitudeDegreeDistance = new()
        {
            { 1, new[] { 110614.71m, 109287.52m } },
            { 2, new[] { 110987.82m, 88374.68m } },
            { 3, new[] { 111258.3m, 69400.28m } },
            { 4, new[] { 110850.0m, 84471.0m } },
            { 5, new[] { 111230.0m, 75027.0m } },
            { 6, new[] { 111230.0m, 79341.0m } },
            { 7, new[] { 111230.0m, 65393.0m } },
            { 8, new[] { 110614.71m, 109287.52m } },
            { 9, new[] { 110614.71m, 109287.52m } },
            { 10, new[] { 110726.0m, 103481.0m } },
            { 11, new[] { 111230.0m, 73167.0m } }
        };

        public List<Turn> NextSegmentsNodeA { get; } = new();
        public List<Turn> NextSegmentsNodeB { get; } = new();

        public List<TrackPoint> Points { get; } = new();

        [JsonIgnore] public TrackPoint A => Points.First();

        [JsonIgnore] public TrackPoint B => Points.Last();

        public string Id { get; set; }

        public void CalculateDistances()
        {
            // We need to set this to 0 when this method is invoked through Slice() as
            // the original first point might be somewhere halfway the source segment.
            Points[0].Index = 0;
            Points[0].Segment = this;
            Points[0].DistanceOnSegment = 0;
            Points[0].DistanceFromLast = 0;

            for (var index = 1; index < Points.Count; index++)
            {
                var previousPoint = Points[index - 1];
                var point = Points[index];

                point.Index = index;

                point.DistanceFromLast = TrackPoint.GetDistanceFromLatLonInMeters(
                    (double)previousPoint.Latitude, (double)previousPoint.Longitude,
                    (double)point.Latitude, (double)point.Longitude);

                point.DistanceOnSegment = previousPoint.DistanceOnSegment + point.DistanceFromLast;

                point.Segment = this;
            }
        }

        public string AsGpx()
        {
            var trkptList = Points
                .Select(x =>
                    $"<trkpt lat=\"{x.Latitude.ToString(CultureInfo.InvariantCulture)}\" lon=\"{x.Longitude.ToString(CultureInfo.InvariantCulture)}\"><ele>{x.Altitude.ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                .ToList();

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<gpx creator=\"Codenizer:ZwiftRouteDownloader\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>{Id}</name>" +
                   $"<desc>{B.DistanceOnSegment}</desc>" +
                   "<type>Strava segment</type>" +
                   $"<trkseg>{string.Join(Environment.NewLine, trkptList)}</trkseg>" +
                   "</trk>" +
                   "</gpx>";
        }

        public Segment Slice(string suffix, int start)
        {
            return Slice(suffix, start, Points.Count);
        }

        public Segment Slice(string suffix, int start, int end)
        {
            var slicedSegement = new Segment
            {
                Id = Id + $"-{suffix}"
            };

            slicedSegement.Points.AddRange(Points.Skip(start).Take(end).ToList());

            // To prevent gaps in sliced segments we want to add the last point of the
            // first slice as the start of the second slice. We need to be use a clone
            // of that point to prevent the index/segment/etc being overwritten on the
            // same reference of that point...
            if (start > 0)
            {
                slicedSegement.Points.Insert(0, Points[start - 1].Clone());
            }

            slicedSegement.CalculateDistances();

            return slicedSegement;
        }

        public SegmentDirection DirectionOf(TrackPoint first, TrackPoint second)
        {
            var firstIndex = Points.IndexOf(first);
            var secondIndex = Points.IndexOf(second);

            if (firstIndex == -1 || secondIndex == -1)
            {
                return SegmentDirection.Unknown;
            }

            return firstIndex < secondIndex
                ? SegmentDirection.AtoB
                : SegmentDirection.BtoA;
        }

        public List<Turn> NextSegments(SegmentDirection segmentDirection)
        {
            if (segmentDirection == SegmentDirection.AtoB)
            {
                return NextSegmentsNodeB;
            }

            if (segmentDirection == SegmentDirection.BtoA)
            {
                return NextSegmentsNodeA;
            }

            throw new ArgumentException(
                "Can't determine next segments for an unknown segment direction",
                nameof(segmentDirection));
        }

        public bool Contains(TrackPoint position)
        {
            return Points.Any(p => p.IsCloseTo(position));
        }

        public void TranslateToGameCoordinates()
        {
            var translatedTrackPoints = Points
                .Select(ToGameCoordinate)
                .ToList();

            Points.Clear();

            Points.AddRange(translatedTrackPoints);
        }

        private static TrackPoint ToGameCoordinate(TrackPoint point)
        {
            var position = LatLongToGameCoordinates(1, point.Latitude, point.Longitude, point.Altitude);

            // Coordinate flipping because Zwift...
            return new TrackPoint(position.Longitude, position.Latitude, point.Altitude);
        }

        private static TrackPoint LatLongToGameCoordinates(int worldId, decimal latitude, decimal longitude,
            decimal altitude)
        {
            decimal f3;
            var fArr = LatitudeLongitudeOffsets[worldId];
            var fArr2 = LatitudeLongitudeDegreeDistance[worldId];
            var f4 = 0.0m;
            if (fArr == null || fArr2 == null)
            {
                return new TrackPoint(0.0m, 0.0m, altitude);
            }

            var f5 = fArr[0] * fArr2[0] * 100.0m;
            var f6 = fArr[1] * fArr2[1] * 100.0m;
            switch (worldId)
            {
                case 1:
                case 2:
                case 9:
                case 10:
                case 11:
                    const decimal f7 = 100;
                    f4 = latitude * fArr2[0] * f7 - f5;
                    f3 = longitude * fArr2[1] * f7 - f6;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    const decimal f8 = 100;
                    f4 = longitude * fArr2[1] * f8 - f6;
                    f3 = f5 - latitude * fArr2[0] * f8;
                    break;
                default:
                    f3 = 0.0m;
                    break;
            }

            return new TrackPoint(f4, f3, altitude);
        }
    }
}