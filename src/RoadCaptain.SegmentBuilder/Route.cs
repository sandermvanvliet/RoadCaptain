using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RoadCaptain.SegmentBuilder
{
    public class Route
    {
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";

        public string Name { get; set; }
        public List<TrackPoint> TrackPoints { get; set; }
        public string Slug { get; set; }

        public static Route FromGpxFile(string filePath)
        {
            var doc = XDocument.Parse(File.ReadAllText(filePath));

            var trkElement = doc.Root.Element(XName.Get("trk", GpxNamespace));

            var trkSeg = trkElement.Elements(XName.Get("trkseg", GpxNamespace));

            var trkpt = trkSeg.Elements(XName.Get("trkpt", GpxNamespace));

            var trackPoints = trkpt
                .Select(trackPoint => new TrackPoint(
                    decimal.Parse(trackPoint.Attribute(XName.Get("lat")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Attribute(XName.Get("lon")).Value, CultureInfo.InvariantCulture),
                    decimal.Parse(trackPoint.Element(XName.Get("ele", GpxNamespace)).Value,
                        CultureInfo.InvariantCulture)
                ))
                .ToList();

            return new Route
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace)).Value,
                Slug = Path.GetFileNameWithoutExtension(filePath),
                TrackPoints = trackPoints
            };
        }

        private static List<Segment> FindOverlappingExistingSegments(TrackPoint point, List<Segment> segments)
        {
            return segments
                .AsParallel()
                .Where(s => s.Points.Any(p => p.IsCloseTo(point)))
                .ToList();
        }

        public List<Segment> SplitToSegments(List<Segment> segments)
        {
            var result = new List<Segment>();

            var currentSegment = new Segment(new List<TrackPoint>()) { Id = $"{Slug}-{result.Count + 1:000}" };
            TrackPoint previousPoint = null;

            foreach (var point in TrackPoints)
            {
                var overlappingExistingSegments = FindOverlappingExistingSegments(point, segments);
                var overlappingNewSegments = result.Where(s => s.Points.Any(p => p.IsCloseTo(point))).ToList();

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
                         currentSegment.Points.Any(p => p.IsCloseTo(point)))
                {
                    // If we find a single match and that was the last added 
                    // point on this segment then we can add the current point.
                    if (currentSegment.B.IsCloseTo(point))
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
                        currentSegment = new Segment(new List<TrackPoint>()) { Id = $"{Slug}-{result.Count + 1:000}" };

                        if (previousPoint != null)
                        {
                            currentSegment.Points.Add(previousPoint);
                        }
                    }

                    currentSegment.Points.Add(point);
                }

                previousPoint = point;
            }

            if (currentSegment != null && currentSegment.Points.Count > 1)
            {
                result.Add(currentSegment);
            }

            foreach (var segment in result)
            {
                segment.CalculateDistances();
            }

            return result;
        }
    }
}