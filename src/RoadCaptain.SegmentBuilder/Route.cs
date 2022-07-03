// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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
        public string[] Sports { get; set; }

        public static Route FromGpxFile(string filePath)
        {
            var doc = XDocument.Parse(File.ReadAllText(filePath));

            var trkElement = doc.Root.Element(XName.Get("trk", GpxNamespace));

            var typeElement = trkElement.Element(XName.Get("link", GpxNamespace))?.Element(XName.Get("type", GpxNamespace));
            var trkSeg = trkElement.Elements(XName.Get("trkseg", GpxNamespace));

            var trkpt = trkSeg.Elements(XName.Get("trkpt", GpxNamespace));

            var trackPoints = trkpt
                .Select(trackPoint => new TrackPoint(
                    double.Parse(trackPoint.Attribute(XName.Get("lat")).Value, CultureInfo.InvariantCulture),
                    double.Parse(trackPoint.Attribute(XName.Get("lon")).Value, CultureInfo.InvariantCulture),
                    double.Parse(trackPoint.Element(XName.Get("ele", GpxNamespace)).Value,
                        CultureInfo.InvariantCulture)
                ))
                .ToList();

            var sports = typeElement?.Value.Split(',') ?? new[] { "running", "cycling" };

            return new Route
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace)).Value,
                Slug = Path.GetFileNameWithoutExtension(filePath),
                TrackPoints = trackPoints,
                Sports = sports
            };
        }

        public List<Segment> SplitToSegments(List<Segment> segments)
        {
            var result = new List<Segment>();

            SportType sport = SportType.Unknown;

            if (Sports.Contains("running") && Sports.Contains("cycling"))
            {
                sport = SportType.Both;
            }
            else if (Enum.TryParse(typeof(SportType), Sports.First(), out var x))
            {
                sport = (SportType)x;
            }

            Segment currentSegment = null;
            TrackPoint previousPoint = null;

            foreach (var currentPoint in TrackPoints)
            {
                // Check if the current point is overlapping with an existing segment.
                var overlappingExistingSegments = segments
                    .Select(segment => new
                    {
                        Segment = segment,
                        OverlappingPoints = segment.Points.Where(point => TrackPointUtils.IsCloseTo(point, currentPoint))
                            .ToList()
                    })
                    .Where(overlap => overlap.OverlappingPoints.Any())
                    .ToList();

                if (overlappingExistingSegments.Any())
                {
                    if (currentSegment == null)
                    {
                        // Progress to the next point until there is no overlap
                        previousPoint = currentPoint;
                        continue;
                    }

                    currentSegment.Points.Add(currentPoint);
                    result.Add(currentSegment);
                    currentSegment = null;

                    previousPoint = currentPoint;
                    continue;
                }

                // Check if the current point is overlapping with a segment we've created
                // for this route. If that exists it means we're intersecting with ourselves
                var overlappingNewSegments = result
                    .Select(segment => new
                    {
                        Segment = segment,
                        OverlappingPoints = segment.Points.Where(point => TrackPointUtils.IsCloseTo(point, currentPoint))
                            .ToList()
                    })
                    .Where(overlap => overlap.OverlappingPoints.Any())
                    .ToList();

                if (overlappingNewSegments.Any())
                {
                    if (currentSegment != null)
                    {
                        if (previousPoint != null)
                        {
                            var bearing = TrackPoint.Bearing(previousPoint, currentPoint);
                            var bearingToOverlap = TrackPoint.Bearing(previousPoint,
                                overlappingNewSegments.First().OverlappingPoints.First());

                            if (Math.Abs(bearing - bearingToOverlap) < 1)
                            {
                                // Heading in the same direction
                                currentSegment.Points.Add(currentPoint);
                                result.Add(currentSegment);
                                currentSegment = null;
                            }
                            else
                            {
                                // At an angle
                                currentSegment.Points.Add(currentPoint);
                                result.Add(currentSegment);
                                currentSegment = null;
                            }
                        }
                    }
                }
                // If no overlap with any existing segments exists start
                // creating a new segment
                else if (currentSegment == null)
                {
                    var trackPoints = new List<TrackPoint>();
                    if (previousPoint != null)
                    {
                        trackPoints.Add(previousPoint);
                    }
                    trackPoints.Add(currentPoint);
                    currentSegment = new Segment(trackPoints) { Id = $"{Slug}-{result.Count + 1:000}", Sport = sport };
                }
                else
                {
                    currentSegment.Points.Add(currentPoint);
                }

                previousPoint = currentPoint;
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
