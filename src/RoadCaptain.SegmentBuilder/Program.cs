using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    class Program
    {
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";

        static void Main(string[] args)
        {
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\zwift-watopia-gpx";

            new Program().Run(gpxDirectory);
        }

        private List<Segment> _segments = new List<Segment>();
        private static readonly double PiRad = Math.PI / 180d;

        public void Run(string gpxDirectory)
        {
            if (!File.Exists(Path.Combine(gpxDirectory, "segments", "snapshot-1.json")))
            {
                /*
                 * - Load the first route
                 * - Create a single segment from that route
                 * - Load the next route
                 * - Walk points and see if there is an existing segment that overlaps
                 *   - If so, ignore this point
                 *   - If not, start building a new segment
                 */
                var gpxFiles = Directory.GetFiles(gpxDirectory, "*.gpx");

                foreach (var filePath in gpxFiles)
                {
                    var route = LoadRouteFromGpx(Path.Combine(gpxDirectory, filePath));

                    Console.WriteLine($"Splitting {route.Slug} into segments");

                    var newSegments = SplitToSegments(route);

                    if (newSegments.Any())
                    {
                        Console.WriteLine($"Found {newSegments.Count} new segments");
                        _segments.AddRange(newSegments);
                    }
                }

                File.WriteAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json"), JsonConvert.SerializeObject(_segments));
            }
            else
            {
                _segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json")));
            }

            // Poplulate distance, index and parent properties
            // of track points on the segment.
            foreach (var segment in _segments)
            {
                segment.CalculateDistances();
            }

            /*
             * When we have a set of segments we can see where we have T-junctions,
             * A route start/end that is close to a point in another segment where
             * that point is somehwere in the middle of that segment.
             * For those matches we want to split up the larger segment.
             */
            foreach (var segment in _segments)
            {
                var startOverlaps = FindOverlappingPointsInSegments(segment.Start);

                foreach (var overlap in startOverlaps)
                {
                    if (overlap.DistanceOnSegment >= 15 &&
                        overlap.Segment.End.DistanceOnSegment - overlap.DistanceOnSegment >= 15)
                    {
                        Console.WriteLine($"Found junction of start of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");
                        
                    }
                }

                var endOverlaps = FindOverlappingPointsInSegments(segment.End);

                foreach (var overlap in endOverlaps)
                {
                    if (overlap.DistanceOnSegment >= 15 &&
                        overlap.Segment.End.DistanceOnSegment - overlap.DistanceOnSegment >= 15)
                    {
                        Console.WriteLine($"Found junction of end of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");
                    }
                }
            }

            //foreach (var segment in _segments)
            //{
            //    File.WriteAllText(
            //        Path.Combine(gpxDirectory, "segments", segment.Id + ".gpx"),
            //        BuildGpx(segment.Id, segment.Points.Count, segment.Points));
            //}
        }

        private List<Segment> SplitToSegments(Route route)
        {
            var result = new List<Segment>();

            var currentSegment = new Segment { Id = $"{route.Slug}-{result.Count + 1:000}" };

            foreach (var point in route.TrackPoints)
            {
                var overlappingExistingSegments = FindOverlappingExistingSegments(point);
                var overlappingNewSegments = result.Where(s => s.Points.Any(p => CloseMatchMeters(p, point))).ToList();

                if (overlappingExistingSegments.Any())
                {
                    // We've found an overlap with an existing route so we can
                    // skip points until we no longer have a match. THat's where
                    // a new segment starts.
                    if (currentSegment != null && currentSegment.Points.Count > 1)
                    {
                        currentSegment.Points.Add(point);
                        result.Add(currentSegment);
                    }

                    currentSegment = null;

                    // TODO: See if the matching point is the start of a segment
                    // If not then we need to split _that_ segment. For now we'll
                    // just ignore that.
                }
                else if (currentSegment != null &&
                         currentSegment.Points.Any(p => CloseMatchMeters(p, point)))
                {
                    // If we find a single match and that was the last added 
                    // point on this segment then we can add the current point.
                    if (CloseMatchMeters(currentSegment.End, point))
                    {
                        currentSegment.Points.Add(point);
                    }
                    else
                    {
                        // We've found an overlap with the current segment so we can
                        // skip points until we no longer have a match. THat's where
                        // a new segment starts.
                        if (currentSegment.Points.Count > 1)
                        {
                            currentSegment.Points.Add(point);
                            result.Add(currentSegment);
                        }

                        currentSegment = null;
                    }
                }
                else if (overlappingNewSegments.Any())
                {
                    // We've found an overlap with a segment of this route that
                    // was detected previously so we can
                    // skip points until we no longer have a match. THat's where
                    // a new segment starts.
                    if (currentSegment != null && currentSegment.Points.Count > 1)
                    {
                        currentSegment.Points.Add(point);
                        result.Add(currentSegment);
                    }

                    currentSegment = null;
                }
                else
                {
                    if (currentSegment == null)
                    {
                        currentSegment = new Segment { Id = $"{route.Slug}-{result.Count + 1:000}" };
                    }

                    currentSegment.Points.Add(point);
                }
            }

            if (currentSegment != null && currentSegment.Points.Count > 1)
            {
                result.Add(currentSegment);
            }

            return result;
        }

        private List<Segment> FindOverlappingExistingSegments(TrackPoint point)
        {
            return _segments
                .AsParallel()
                .Where(s => s.Points.Any(p => CloseMatchMeters(p, point)))
                .ToList();
        }

        private List<TrackPoint> FindOverlappingPointsInSegments(TrackPoint point)
        {
            return _segments
                .AsParallel()
                .Select(segment => segment.Points.Where(p => CloseMatchMeters(p, point)))
                .Where(points => points.Any())
                .SelectMany(points => points)
                .ToList();
        }

        private static bool CloseMatchMeters(TrackPoint a, TrackPoint b)
        {
            var distance = GetDistanceFromLatLonInMeters(
                (double)a.Latitude, (double)a.Longitude,
                (double)b.Latitude, (double)b.Longitude);

            if (distance < 15 && Math.Abs(a.Altitude - b.Altitude) <= 1)
            {
                return true;
            }

            return false;
        }

        public static decimal GetDistanceFromLatLonInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km

            return (decimal)d * 1000;
        }

        static double Deg2Rad(double deg)
        {
            return deg * PiRad;
        }

        private static string BuildGpx(string name, double distance, List<TrackPoint> points)
        {
            var trkptList = points
                .Select((x, index) => $"<trkpt lat=\"{x.Latitude.ToString(CultureInfo.InvariantCulture)}\" lon=\"{x.Longitude.ToString(CultureInfo.InvariantCulture)}\"><ele>{x.Altitude.ToString(CultureInfo.InvariantCulture)}</ele></trkpt>")
                .ToList();

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<gpx creator=\"Codenizer:ZwiftRouteDownloader\" version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                   "<trk>" +
                   $"<name>{name}</name>" +
                   $"<desc>{distance}</desc>" +
                   "<type>Strava segment</type>" +
                   $"<trkseg>{string.Join(Environment.NewLine, trkptList)}</trkseg>" +
                   "</trk>" +
                   "</gpx>";
        }

        private static Route LoadRouteFromGpx(string filePath)
        {
            var doc = XDocument.Parse(File.ReadAllText(filePath));

            var trkElement = doc.Root.Element(XName.Get("trk", GpxNamespace));

            var trkSeg = trkElement.Elements(XName.Get("trkseg", GpxNamespace));

            var trkpt = trkSeg.Elements(XName.Get("trkpt", GpxNamespace));

            var trackPoints = trkpt
                .Select(trackPoint => new TrackPoint(
                    decimal.Parse(trackPoint.Attribute(XName.Get("lat")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Attribute(XName.Get("lon")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Element(XName.Get("ele", GpxNamespace)).Value, CultureInfo.InvariantCulture)
                    ))
                .ToList();

            return new Route
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace)).Value,
                Slug = Path.GetFileNameWithoutExtension(filePath),
                TrackPoints = trackPoints
            };
        }
    }

    internal class Segment
    {
        public List<TrackPoint> Points { get; } = new();
        public TrackPoint Start => Points.First();
        public TrackPoint End => Points.Last();
        public string Id { get; set; }

        public void CalculateDistances()
        {
            for (var index = 1; index < Points.Count; index++)
            {
                var previousPoint = Points[index - 1];
                var point = Points[index];

                point.Index = index;

                point.DistanceFromLast = Program.GetDistanceFromLatLonInMeters(
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
    }

    internal class Route
    {
        public string Name { get; set; }
        public List<TrackPoint> TrackPoints { get; set; }
        public string Slug { get; set; }
    }

    internal class TrackPoint
    {
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
        public Segment Segment { get; set; }

        public override string ToString()
        {
            return $"{Latitude} x {Longitude}";
        }
    }
}
