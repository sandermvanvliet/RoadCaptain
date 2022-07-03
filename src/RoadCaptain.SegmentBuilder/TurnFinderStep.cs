using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RoadCaptain.Adapters;

namespace RoadCaptain.SegmentBuilder
{
    internal class TurnFinderStep
    {
        public static void Run(List<Segment> segments, string gpxDirectory)
        {
            foreach (var segment in segments)
            {
                // Clear turns from segments otherwise it blows up because
                // loading the segments applies the turns to the segments
                segment.NextSegmentsNodeA.Clear();
                segment.NextSegmentsNodeB.Clear();
            }

            GenerateTurns(segments);

            var turns = segments
                .Select(segment => new SegmentTurns
                {
                    SegmentId = segment.Id,
                    TurnsA = TurnsFromSegment(segment.NextSegmentsNodeA),
                    TurnsB = TurnsFromSegment(segment.NextSegmentsNodeB)
                })
                .ToList();
            
            File.WriteAllText(
                Path.Combine(gpxDirectory, "segments", "turns.json"),
                JsonConvert.SerializeObject(turns, Formatting.Indented, Program.SerializerSettings));
        }

        private static void GenerateTurns(List<Segment> segments)
        {
            foreach (var segment in segments)
            {
                FindOverlapsWithSegmentNode(segments, segment, segment.A, segment.NextSegmentsNodeA);

                FindOverlapsWithSegmentNode(segments, segment, segment.B, segment.NextSegmentsNodeB);
            }
        }

        private static void FindOverlapsWithSegmentNode(List<Segment> segments, Segment segment, TrackPoint endPoint, List<Turn> endNode)
        {
            if (endNode.Count > 0)
            {
                Debugger.Break();
            }

            var overlaps = OverlapsWith(endPoint, segments, segment.Id);

            if (!overlaps.Any())
            {
                overlaps = OverlapsWith(endPoint, segments, segment.Id, 30);
            }

            var pointBeforeEndPoint = endPoint.Index.Value == 0
                ? segment.Points[1]
                : segment.Points[endPoint.Index.Value - 1];

            var segmentEndBearing = TrackPoint.Bearing(pointBeforeEndPoint, endPoint);

            foreach (var overlap in overlaps)
            {
                var bearing = TrackPoint.Bearing(
                    endPoint, 
                    TrackPointUtils.IsCloseTo(endPoint, overlap.A) ? overlap.A : overlap.B);

                var turnDirection = TurnDirectionFromBearings(segmentEndBearing, bearing);

                if (endNode.All(n => n.SegmentId != overlap.Id))
                {
                    var existing = endNode.SingleOrDefault(n => n.Direction == turnDirection);
                    if (existing != null)
                    {
                        Console.WriteLine($"Already have a turn for {turnDirection} which goes to {existing.SegmentId}");
                    }
                    else
                    {
                        endNode.Add(new Turn(turnDirection, overlap.Id));
                    }
                }
            }

            if (endNode.Select(n => n.Direction).Distinct().Count() != endNode.Count)
            {
                Debugger.Break();
            }
        }

        private static TurnDirection TurnDirectionFromBearings(double segmentEndBearing, double bearingToNextSegment)
        {
            // Given:
            // - segmentEndBearing is treated as North - 0 degrees
            // - calculate offset from 0 degrees
            // - apply offset to bearingToNextSegment
            // - determine direction based on bearingToNextSegment

            var correctedBearingToNextSegment = bearingToNextSegment - segmentEndBearing;

            if (correctedBearingToNextSegment > 15 && correctedBearingToNextSegment < 165)
            {
                return TurnDirection.Right;
            }
            
            if (correctedBearingToNextSegment > 195 && correctedBearingToNextSegment < 345)
            {
                return TurnDirection.Left;
            }

            return TurnDirection.GoStraight;
        }

        public static List<Segment> OverlapsWith(TrackPoint point, List<Segment> segments, string currentSegmentId, int radiusMeters = 15)
        {
            return segments
                .Where(s => s.Id != currentSegmentId)
                .Where(s => TrackPointUtils.IsCloseTo(s.A, point, radiusMeters) || TrackPointUtils.IsCloseTo(s.B, point, radiusMeters))
                .ToList();
        }

        private static SegmentTurn TurnsFromSegment(List<Turn> turns)
        {
            var turn = new SegmentTurn();

            var left = turns.SingleOrDefault(t => t.Direction == TurnDirection.Left);
            if (left != null)
            {
                turn.Left = left.SegmentId;
            }
            var goStraight = turns.SingleOrDefault(t => t.Direction == TurnDirection.GoStraight);
            if (goStraight != null)
            {
                turn.GoStraight = goStraight.SegmentId;
            }
            var right = turns.SingleOrDefault(t => t.Direction == TurnDirection.Right);
            if (right != null)
            {
                turn.Right = right.SegmentId;
            }

            return turn;
        }
    }
}