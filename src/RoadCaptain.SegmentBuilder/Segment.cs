using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    public class Segment
    {
        public List<TrackPoint> Points { get; } = new();
        [JsonIgnore]
        public TrackPoint Start => Points.First();
        [JsonIgnore]
        public TrackPoint End => Points.Last();
        public string Id { get; set; }

        public void CalculateDistances()
        {
            for (var index = 1; index < Points.Count; index++)
            {
                var previousPoint = Points[index - 1];
                var point = Points[index];

                point.Index = index;

                point.DistanceFromLast = TrackPoint.GetDistanceFromLatLonInMeters(
                    (double)previousPoint.Latitude, (double)previousPoint.Longitude,
                    (double)point.Latitude, (double)point.Longitude);

                point.DistanceOnSegment = previousPoint.DistanceOnSegment + point.DistanceFromLast;

                if (index == 1)
                {
                    previousPoint.Segment = this;
                }

                point.Segment = this;
            }
        }

        public string AsGpx()
        {
            var trkptList = Points
                .Select(x => $"<trkpt lat=\"{x.Latitude.ToString(CultureInfo.InvariantCulture)}\" lon=\"{x.Longitude.ToString(CultureInfo.InvariantCulture)}\"><ele>{x.Altitude.ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                .ToList();

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<gpx creator=\"Codenizer:ZwiftRouteDownloader\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>{Id}</name>" +
                   $"<desc>{End.DistanceOnSegment}</desc>" +
                   "<type>Strava segment</type>" +
                   $"<trkseg>{string.Join(Environment.NewLine, trkptList)}</trkseg>" +
                   "</trk>" +
                   "</gpx>";
        }
    }
}