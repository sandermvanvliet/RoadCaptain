using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain
{
    public class Segment
    {
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
            // If Index is provided use that instead of looking
            // up the index of each point in the segment.
            if (first.Index.HasValue && second.Index.HasValue)
            {
                return first.Index < second.Index
                    ? SegmentDirection.AtoB
                    : SegmentDirection.BtoA;
            }

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

        public TrackPoint GetClosestPositionOnSegment(TrackPoint position)
        {
            return Points
                .Select(p => new { Point = p, Distance = p.DistanceTo(position) })
                .OrderBy(x => x.Distance)
                .FirstOrDefault()
                ?.Point;
        }
    }
}