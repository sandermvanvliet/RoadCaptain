// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class TurnFinderStep : BaseStep
    {
        public override Context Run(Context context)
        {
            var segments = context.Segments.ToList();

            // Clear node turns from each segment to ensure
            // we're not stuck with some pre-existing turns
            // from before this step has run.
            foreach (var segment in segments)
            {
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
                Path.Combine(context.GpxDirectory, "segments", $"turns-{context.World}.json"),
                JsonConvert.SerializeObject(turns.OrderBy(t => t.SegmentId).ToList(), Formatting.Indented, Program.SerializerSettings));

            return new Context(Step, segments, context.GpxDirectory, context.World);
        }

        private void GenerateTurns(List<Segment> segments)
        {
            foreach (var segment in segments)
            {
                FindOverlapsWithSegmentNode(segments, segment, segment.A, segment.NextSegmentsNodeA);

                FindOverlapsWithSegmentNode(segments, segment, segment.B, segment.NextSegmentsNodeB);
            }
        }

        private void FindOverlapsWithSegmentNode(List<Segment> segments, Segment segment, TrackPoint endPoint, List<Turn> endNode)
        {
            var radiusMeters = 25;

            if (endNode.Count > 0)
            {
                Debugger.Break();
            }

            var overlaps = OverlapsWith(endPoint, segments, segment.Id, radiusMeters);

            if (!overlaps.Any())
            {
                overlaps = OverlapsWith(endPoint, segments, segment.Id, 30);
            }

            var pointBeforeEndPoint = endPoint.Index.Value == 0
                ? segment.Points[1]
                : segment.Points[^2];

            var segmentEndBearing = TrackPoint.Bearing(pointBeforeEndPoint, endPoint);

            // Single overlap always means GoStraight
            if (overlaps.Count == 1)
            {
                endNode.Add(new Turn(TurnDirection.GoStraight, overlaps[0].Id));
                return;
            }

            foreach (var overlap in overlaps)
            {
                var endPointOfOverlap = TrackPointUtils.IsCloseTo(endPoint, overlap.A, radiusMeters) ? overlap.A : overlap.B;
                var nextPointOfOverlap = endPointOfOverlap.Index == 0 ? overlap.Points[1] : overlap.Points[^2];

                var bearing = TrackPoint.Bearing(
                    endPointOfOverlap,
                    nextPointOfOverlap);

                var turnDirection = TurnDirectionFromBearings(segmentEndBearing, bearing);

                if (endNode.All(n => n.SegmentId != overlap.Id))
                {
                    var existing = endNode.SingleOrDefault(n => n.Direction == turnDirection);
                    if (existing != null)
                    {
                        Logger.Information($"Already have a turn for {turnDirection} which goes to {existing.SegmentId}");
                    }
                    else
                    {
                        Logger.Information($"Adding turn {turnDirection} to {overlap.Id}");
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

            var offset = 360 - segmentEndBearing;
            var correctedBearingToNextSegment = (bearingToNextSegment  + offset) % 360;

            if (correctedBearingToNextSegment > 25 && correctedBearingToNextSegment < 155)
            {
                return TurnDirection.Right;
            }

            if (correctedBearingToNextSegment > 225 && correctedBearingToNextSegment < 335)
            {
                return TurnDirection.Left;
            }

            return TurnDirection.GoStraight;
        }

        public static List<Segment> OverlapsWith(TrackPoint point, IEnumerable<Segment> segments, string currentSegmentId, int radiusMeters = 15)
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

        public TurnFinderStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}
