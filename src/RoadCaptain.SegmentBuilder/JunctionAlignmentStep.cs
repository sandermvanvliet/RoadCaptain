using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.SegmentBuilder
{
    internal class JunctionAlignmentStep
    {
        public static void Run(List<Segment> segments)
        {
            foreach (var segment in segments)
            {
                var segmentsExceptSegmentToAdjust = segments.Where(s => s.Id != segment.Id).ToList();

                AdjustNodeA(
                    segment,
                    segmentsExceptSegmentToAdjust);

                AdjustNodeB(
                    segment,
                    segmentsExceptSegmentToAdjust);
            }
        }

        private static void AdjustNodeA(Segment segmentToAdjust, List<Segment> segmentsExceptSegmentToAdjust)
        {
            Console.WriteLine($"Adjusting segment {segmentToAdjust.Id} node A");

            var overlaps = segmentsExceptSegmentToAdjust
                .Select(segment => new
                {
                    Segment = segment,
                    OverlappingPoints = segment
                        .Points
                        .Where(point => TrackPointUtils.IsCloseTo(point, segmentToAdjust.A, 30) &&
                                        (point.DistanceOnSegment > 100 ||
                                         point.DistanceOnSegment < segment.Distance - 100))
                        .ToList()
                })
                .Where(overlap => overlap.OverlappingPoints.Any())
                .Select(overlap => new
                {
                    overlap.Segment,
                    ClosestPoint = overlap
                        .OverlappingPoints
                        .Select(p => new
                        {
                            Point = p,
                            Distance = TrackPoint.GetDistanceFromLatLonInMeters(p.Latitude, p.Longitude,
                                segmentToAdjust.A.Latitude, segmentToAdjust.A.Longitude)
                        })
                        .OrderBy(x => x.Distance)
                        .First()
                })
                .ToList();

            if (!overlaps.Any())
            {
                Console.WriteLine("\tDid not find overlaps!");
                return;
            }
            
            var junctionSegment = overlaps.OrderBy(overlap => overlap.ClosestPoint.Distance).First();

            Console.WriteLine($"\tFound overlap with {junctionSegment.Segment.Id}");

            var closestPoint = junctionSegment.ClosestPoint.Point;

            // From closest point:
            var startIndex = closestPoint.Index.Value;
            var endPoint = segmentToAdjust.Points[1];
            
            var down = new List<Tuple<TrackPoint, double>>();

            // walk UP the junction segment and calculate distance
            // from endpoint' to current point and store the result
            // do this for 10 points
            foreach(var point in junctionSegment.Segment.Points.Skip(startIndex).Take(20))
            {
                down.Add(new Tuple<TrackPoint, double>(
                    point,
                    TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude, endPoint.Latitude, endPoint.Longitude)));
            }

            // from both result sets find the point where the distance
            // is the smallest
            down = down.OrderBy(d => d.Item2).ToList();
            var newPoint = down[0].Item1;
            
            // Next: 
            // for -1 / +1 of new point index interpolate
            // new points and again find the closest distance
            // to endpoint'
            var newPointIndex = newPoint.Index.Value;
            if (newPointIndex == junctionSegment.Segment.Points.Count - 1)
            {
                return;
            }

            var interpolateStart = junctionSegment.Segment.Points[newPointIndex - 1];
            var interpolateEnd = junctionSegment.Segment.Points[newPointIndex + 1];

            var interpolatedPoints = Interpolate(interpolateStart, newPoint, 5)
                .Union(Interpolate(newPoint, interpolateEnd, 5))
                .ToList();
            
            down.Clear();

            foreach (var point in interpolatedPoints)
            {
                down.Add(new Tuple<TrackPoint, double>(
                    point,
                    TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude, endPoint.Latitude, endPoint.Longitude)));
            }

            // from both result sets find the point where the distance
            // is the smallest
            down = down.OrderBy(d => d.Item2).ToList();
            newPoint = down[0].Item1;
            newPoint.Index = segmentToAdjust.A.Index;

            // Replace original endpoint with new one
            var originalA = segmentToAdjust.A.Clone();
            
            Console.WriteLine($"\tReplacing {originalA.CoordinatesDecimal} with {newPoint.CoordinatesDecimal}");

            segmentToAdjust.Points.RemoveAt(0);
            segmentToAdjust.Points.Insert(0, newPoint);
        }

        private static void AdjustNodeB(Segment segmentToAdjust, List<Segment> segmentsExceptSegmentToAdjust)
        {
            Console.WriteLine($"Adjusting segment {segmentToAdjust.Id} node B");

            var overlaps = segmentsExceptSegmentToAdjust
                .Select(segment => new
                {
                    Segment = segment,
                    OverlappingPoints = segment
                        .Points
                        .Where(point => TrackPointUtils.IsCloseTo(point, segmentToAdjust.B))
                        .ToList()
                })
                .Where(overlap => overlap.OverlappingPoints.Any())
                .Select(overlap => new 
                {
                    overlap.Segment,
                    ClosestPoint = overlap.OverlappingPoints.Select(p => new
                    {
                        Point = p,
                        Distance = TrackPoint.GetDistanceFromLatLonInMeters(p.Latitude, p.Longitude, segmentToAdjust.B.Latitude, segmentToAdjust.B.Longitude)
                    })
                        .OrderBy(x => x.Distance)
                        .First()
                })
                .ToList();

            if (!overlaps.Any())
            {
                Console.WriteLine("\tDid not find overlaps!");
                return;
            }
            
            var junctionSegment = overlaps.OrderBy(overlap => overlap.ClosestPoint.Distance).First();

            Console.WriteLine($"\tFound overlap with {junctionSegment.Segment.Id}");

            var closestPoint = junctionSegment.ClosestPoint.Point;

            // From closest point:
            var startIndex = closestPoint.Index.Value;
            var endPoint = segmentToAdjust.Points[^2];
            
            var down = new List<Tuple<TrackPoint, double>>();

            // walk UP the junction segment and calculate distance
            // from endpoint' to current point and store the result
            // do this for 10 points
            foreach(var point in junctionSegment.Segment.Points.Skip(startIndex - 20).Take(20))
            {
                down.Add(new Tuple<TrackPoint, double>(
                    point,
                    TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude, endPoint.Latitude, endPoint.Longitude)));
            }

            // from both result sets find the point where the distance
            // is the smallest
            down = down.OrderBy(d => d.Item2).ToList();
            var newPoint = down[0].Item1;
            
            // Next: 
            // for -1 / +1 of new point index interpolate
            // new points and again find the closest distance
            // to endpoint'
            var newPointIndex = newPoint.Index.Value;

            // Deal with segments that join head-on
            if (newPointIndex == junctionSegment.Segment.Points.Count - 1 || newPointIndex == 0)
            {
                return;
            }

            var interpolateStart = junctionSegment.Segment.Points[newPointIndex - 1];
            var interpolateEnd = junctionSegment.Segment.Points[newPointIndex + 1];

            var interpolatedPoints = Interpolate(interpolateStart, newPoint, 5)
                .Union(Interpolate(newPoint, interpolateEnd, 5))
                .ToList();
            
            down.Clear();

            foreach (var point in interpolatedPoints)
            {
                down.Add(new Tuple<TrackPoint, double>(
                    point,
                    TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude, endPoint.Latitude, endPoint.Longitude)));
            }

            // from both result sets find the point where the distance
            // is the smallest
            down = down.OrderBy(d => d.Item2).ToList();
            newPoint = down[0].Item1;
            newPoint.Index = segmentToAdjust.B.Index;

            // Replace original endpoint with new one
            var originalA = segmentToAdjust.B.Clone();
            
            Console.WriteLine($"\tReplacing {originalA.CoordinatesDecimal} with {newPoint.CoordinatesDecimal}");

            segmentToAdjust.Points.RemoveAt(newPoint.Index.Value);
            segmentToAdjust.Points.Add( newPoint);
        }

        private static List<TrackPoint> Interpolate(TrackPoint start, TrackPoint end, int step)
        {
            var deltaLat = Math.Abs(start.Latitude - end.Latitude);
            var deltaLon = Math.Abs(start.Longitude - end.Longitude);
            var deltaAlt = Math.Abs(start.Altitude - end.Altitude);

            var stepLat = deltaLat / step;
            if (start.Latitude > end.Latitude && stepLat > 0)
            {
                stepLat = -stepLat;
            }

            var stepLon = deltaLon / step;
            if (start.Longitude > end.Longitude && stepLon > 0)
            {
                stepLon = -stepLon;
            }

            var stepAlt = deltaAlt / step;
            if (start.Altitude > end.Altitude && stepAlt > 0)
            {
                stepAlt = -stepAlt;
            }

            var result = new List<TrackPoint>();

            var x = start;

            for (var i = 0; i < step; i++)
            {
                var newPoint = new TrackPoint(
                    x.Latitude + stepLat,
                    x.Longitude + stepLon,
                    x.Altitude + stepAlt
                );

                result.Add(newPoint);

                x = newPoint;
            }

            return result;
        }
    }
}