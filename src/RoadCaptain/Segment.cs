// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace RoadCaptain
{
    public class Segment
    {
        private double? _ascent;
        private double? _descent;
        private double? _distance;
        private string _name;
        
        public string Id { get; set; }
        public List<TrackPoint> Points { get; }
        public SportType Sport { get; set; } = SportType.Both;
        public SegmentType Type { get; set; } = SegmentType.Segment;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = Id.Replace("watopia-", "");
                }

                return _name;
            }
            set => _name = value;
        }

        public string NoSelectReason { get; set; }
        [JsonIgnore] public TrackPoint A => Points.First();
        [JsonIgnore] public TrackPoint B => Points.Last();
        [JsonIgnore] public List<Turn> NextSegmentsNodeA { get; } = new();
        [JsonIgnore] public List<Turn> NextSegmentsNodeB { get; } = new();

        [JsonIgnore] public BoundingBox BoundingBox { get; }

        [JsonIgnore] public double Distance
        {
            get
            {
                if (_distance == null)
                {
                    _distance = Points.Sum(p => p.DistanceFromLast);
                }

                return _distance.GetValueOrDefault();
            }
        }

        [JsonIgnore] public double Ascent
        {
            get
            {
                if (_ascent == null)
                {
                    CalculateAscentAndDescent();
                }

                return _ascent.GetValueOrDefault();
            }
        }

        [JsonIgnore] public double Descent
        {
            get
            {
                if (_descent == null)
                {
                    CalculateAscentAndDescent();
                }

                return _descent.GetValueOrDefault();
            }
        }

        private void CalculateAscentAndDescent()
        {
            var ascent = 0d;
            var descent = 0d;

            for (var index = 1; index < Points.Count; index++)
            {
                var delta = Points[index].Altitude - Points[index - 1].Altitude;

                if (delta < 0)
                {
                    descent += Math.Abs(delta);
                }
                else if(delta > 0)
                {
                    ascent += delta;
                }
            }

            _ascent = ascent;
            _descent = descent;
        }

        public Segment(List<TrackPoint> points)
        {
            Points = points;
            BoundingBox = BoundingBox.From(Points);
        }

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
                    previousPoint.Latitude, previousPoint.Longitude,
                    point.Latitude, point.Longitude);

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
        
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";
        public static Segment FromGpx(string gpxFileContents)
        {
            var doc = XDocument.Parse(gpxFileContents);

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

            var sports = typeElement?.Value.Split(',') ?? new [] { "running", "cycling" };

            SportType sport = SportType.Unknown;
            SegmentType segmentType = SegmentType.Segment;

            if(sports.Contains("running") && sports.Contains("cycling"))
            {
                sport = SportType.Both;
            }
            else if(Enum.TryParse(typeof(SportType), sports.First(), true, out var parsedSport))
            {
                sport = (SportType)parsedSport;
            }
            else if(Enum.TryParse(typeof(SegmentType), sports.First(), true, out var parsedSegmentType))
            {
                segmentType = (SegmentType)parsedSegmentType;
            }

            var segment = new Segment(trackPoints)
            {
                Name = trkElement.Element(XName.Get("name", GpxNamespace)).Value,
                Sport = sport,
                Type = segmentType
            };

            segment.CalculateDistances();
            segment.CalculateAscentAndDescent();

            return segment;
        }

        public Segment Slice(string suffix, int start)
        {
            return Slice(suffix, start, Points.Count);
        }

        public Segment Slice(string suffix, int start, int end)
        {
            var slicedPoints = Points.Skip(start).Take(end).ToList();

            var slicedSegement = new Segment(slicedPoints)
            {
                Id = Id + $"-{suffix}",
                Sport = Sport,
                Name = Name + $"-{suffix}"
            };

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
            // Short-circuit matching by first checking against the bounding box of this segment
            if (!BoundingBox.IsIn(position))
            {
                return false;
            }
            
            // Some segments may have a lot of points, especially on longer
            // segments. Because the points are in sequence but not ordered
            // based on their values it means that we need to iterate over
            // the entire collection.
            // To speed up this process the lookup will proceed from both
            // ends of the collection so that we reduce the amount of
            // iterations that would be needed if the point we seek is at
            // the end of the Points collection.
            // If index and reverseIndex converge we exit as in that case
            // we haven't found a match.
            var reverseIndex = Points.Count - 1;
            for (var index = 0; index < Points.Count; index++)
            {
                if (Points[index].IsCloseTo(position) ||
                    Points[reverseIndex--].IsCloseTo(position))
                {
                    return true;
                }
                
                // Note: Use greater-than-or-equal to deal with collections
                //       that have an odd number of items as in that case
                //       the indexes won't ever be equal and we'd still
                //       iterate over the entire collection.
                if (index >= reverseIndex)
                {
                    break;
                }
            }

            return false;
        }
    }
}
