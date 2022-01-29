using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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

        private readonly List<Segment> _segments = new List<Segment>();
        private static readonly double PiRad = Math.PI / 180d;

        public void Run(string gpxDirectory)
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

            foreach (var segment in _segments)
            {
                File.WriteAllText(
                    Path.Combine(gpxDirectory, "segments", segment.Id + ".gpx"),
                    BuildGpx(segment.Id, segment.Points.Count, segment.Points));
            }
        }

        private List<Segment> SplitToSegments(Route route)
        {
            var result = new List<Segment>();

            var currentSegment = new Segment { Id = $"{route.Slug}-{result.Count + 1:000}" };

            foreach (var point in route.TrackPoints)
            {
                if (_segments.Any(s => s.Points.Any(p => CloseMatchMeters(p, point))))
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
                    var matches = currentSegment.Points.Where(p => CloseMatchMeters(p, point)).ToList();
                    
                    // If we find a single match and that was the last added 
                    // point on this segment then we can add the current point.
                    if (matches.Count == 1 && matches.Single() == currentSegment.End)
                    {
                        currentSegment.Points.Add(point);
                    }
                    else if (matches.Count > 1)
                    {
                        Debugger.Break();
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
                else if (result.Any(s => s.Points.Any(p => CloseMatchMeters(p, point))))
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

        private static bool CloseMatchMeters(TrackPoint a, TrackPoint b)
        {
            if (CloseMatch(a, b, 0.001m, 0.001m))
            {
                var distance = GetDistanceFromLatLonInMeters((double)a.Latitude, (double)a.Longitude,
                    (double)b.Latitude, (double)b.Longitude);

                if (distance < 10)
                {
                    return true;
                }
            }

            return false;
        }

        private static decimal GetDistanceFromLatLonInMeters(double lat1, double lon1, double lat2, double lon2)
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

        private static bool CloseMatch(TrackPoint x, TrackPoint y, decimal marginLat, decimal marginLon)
        {
            // Should do something fancy with overlapping circles or something
            return Math.Abs(x.Latitude - y.Latitude) < marginLat &&
                   Math.Abs(x.Longitude - y.Longitude) < marginLon &&
                   Math.Abs(x.Altitude - y.Altitude) < 1m;
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

        private static void FindIntersections(Route routeOne, Route routeTwo)
        {
            var routeOneBox = routeOne.BoundingBox();
            var routeTwoBox = routeTwo.BoundingBox();

            Console.WriteLine("Box 1 " + routeOneBox);
            Console.WriteLine("Box 2 " + routeTwoBox);

            if (routeOneBox.Overlaps(routeTwoBox))
            {
                var overlaps = SubdivideAndFindOverlaps(routeOneBox, routeTwoBox);

                foreach (var o in overlaps)
                {
                    foreach (var overlapWith in o.OverlapsWith)
                    {
                        var subs = SubdivideAndFindOverlaps(o.Box, overlapWith);


                    }
                }
            }
            else
            {
                Console.WriteLine("Doesn't overlap");
            }
        }

        private static List<BoxOverlap> SubdivideAndFindOverlaps(Box routeOneBox, Box routeTwoBox)
        {
            var boxesOne = routeOneBox.Subdivide();
            var boxesTwo = routeTwoBox.Subdivide();

            var overlaps = new List<BoxOverlap>();

            foreach (var box in boxesOne)
            {
                Console.WriteLine(box);

                var boxOverlaps = boxesTwo
                    .Where(x => box.Overlaps(x))
                    .ToList();

                if (boxOverlaps.Any())
                {
                    overlaps.Add(new BoxOverlap
                    {
                        Box = box,
                        OverlapsWith = boxOverlaps
                    });
                }
            }

            return overlaps;
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
        public List<TrackPoint> Points { get; } = new List<TrackPoint>();
        public TrackPoint Start => Points.First();
        public TrackPoint End => Points.Last();
        public string Id { get; set; }
    }

    internal class BoxOverlap
    {
        public Box Box { get; set; }
        public List<Box> OverlapsWith { get; set; }
    }

    internal class Route
    {
        public string Name { get; set; }
        public List<TrackPoint> TrackPoints { get; set; }
        public string Slug { get; set; }

        public Box BoundingBox()
        {
            var minLon = TrackPoints.Min(t => t.Longitude);
            var minLat = TrackPoints.Min(t => t.Latitude);

            var maxLon = TrackPoints.Max(t => t.Longitude);
            var maxLat = TrackPoints.Max(t => t.Latitude);

            if (minLat < 0 && maxLat < 0)
            {
                (minLat, maxLat) = (maxLat, minLat);
            }

            return new Box
            {
                X1 = minLon,
                Y1 = minLat,
                X2 = maxLon,
                Y2 = maxLat,
                Id = "Main",
                Level = 1
            };
        }
    }

    internal class Box
    {
        public decimal X1 { get; set; }
        public decimal Y1 { get; set; }
        public decimal X2 { get; set; }
        public decimal Y2 { get; set; }
        public string Id { get; set; }
        public int Level { get; set; }

        public bool Overlaps(Box other)
        {
            if (X1 <= other.X2 && X2 >= other.X1)
            {
                // Because negative coordinates...
                if (Y1 >= other.Y2 && Y2 <= other.Y1)
                {
                    return true;
                }
            }

            return false;
        }

        public Box[] Subdivide()
        {
            var width = X2 - X1;
            var height = Y2 - Y1;
            var centerX = width / 2;
            var centerY = height / 2;

            return new[]
            {
                new Box
                {
                    Id = "LeftTop",
                    X1 = X1,
                    Y1 = Y1,
                    X2 = X1 + centerX,
                    Y2 = Y1 + centerY,
                    Level = Level + 1
                },
                new Box
                {
                    Id = "RightTop",
                    X1 = X1 + centerX,
                    Y1 = Y1,
                    X2 = X2,
                    Y2 = Y1 + centerY,
                    Level = Level + 1
                },
                new Box
                {
                    Id = "LeftBottom",
                    X1 = X1,
                    Y1 = Y1 + centerY,
                    X2 = X1 + centerX,
                    Y2 = Y2,
                    Level = Level + 1
                },
                new Box
                {
                    Id = "RightBottom",
                    X1 = X1 + centerX,
                    Y1 = Y1 + centerY,
                    X2 = X2,
                    Y2 = Y2,
                    Level = Level + 1
                },
            };
        }

        public override string ToString()
        {
            return $"[{Id}] ({Level}) {X1} x {Y1} => {X2} x {Y2}";
        }
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

        public override string ToString()
        {
            return $"{Latitude} x {Longitude}";
        }
    }
}
