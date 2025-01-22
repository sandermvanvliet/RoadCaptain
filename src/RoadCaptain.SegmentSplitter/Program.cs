// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Adapters;

namespace RoadCaptain.SegmentSplitter
{
    public class Program
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public static void Main(string[] args)
        {
            var segmentFileName = args[0];
            var turnsFileName = args[1];
            var segmentToSplitId = args[2];
            var splitPoint = args[3];

            new Program().Split(segmentToSplitId, segmentFileName, turnsFileName, splitPoint);
        }

        public void Split(string segmentToSplitId, string segmentFileName, string turnsFileName, string splitPoint)
        {
            var segments =
                JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(segmentFileName), _serializerSettings);
            
            var turns = 
                JsonConvert.DeserializeObject<List<SegmentTurns>>(File.ReadAllText(turnsFileName), _serializerSettings);

            var segmentToSplit = segments.SingleOrDefault(s => s.Id == segmentToSplitId);

            if (segmentToSplit == null)
            {
                throw new Exception($"Segment '{segmentToSplitId}' not found");
            }

            var sliceIndex =
                segmentToSplit.Points.FindIndex(trackPoint => trackPoint.CoordinatesDecimal == splitPoint);

            if (sliceIndex == -1)
            {
                Console.WriteLine("Split point not found on segment, exiting...");
                return;
            }
            
            var beforeSplit = segmentToSplit.Slice("before", 0, sliceIndex);
            var afterSplit = segmentToSplit.Slice("after", sliceIndex);

            Console.WriteLine($"Split {segmentToSplit.Id} into {beforeSplit.Id} and {afterSplit.Id}");
            
            beforeSplit.CalculateDistances();
            afterSplit.CalculateDistances();

            segments.Remove(segmentToSplit);
            segments.Add(beforeSplit);
            segments.Add(afterSplit);

            var beforeTurns = new SegmentTurns
            {
                SegmentId = beforeSplit.Id,
                TurnsA = new SegmentTurn
                {
                    // Copy from turns of segmentToSplit
                },
                TurnsB = new SegmentTurn
                {
                    GoStraight = afterSplit.Id
                }
            };

            var afterTurns = new SegmentTurns
            {
                SegmentId = afterSplit.Id,
                TurnsA = new SegmentTurn
                {
                    GoStraight = beforeSplit.Id
                },
                TurnsB = new SegmentTurn
                {
                    // Copy from turns of segmentToSplit
                }
            };

            var originalTurnsOfSegment = turns.Single(t => t.SegmentId == segmentToSplit.Id);

            // Before
            Dwim(turns, originalTurnsOfSegment.TurnsA, beforeTurns.TurnsA, segmentToSplit.Id, beforeSplit.Id);

            // After
            Dwim(turns, originalTurnsOfSegment.TurnsB, afterTurns.TurnsB, segmentToSplit.Id, afterSplit.Id);

            // Add new turns
            turns.Add(beforeTurns);
            turns.Add(afterTurns);

            // Remove the old one as that segment doesn't exist anymore
            turns.Remove(originalTurnsOfSegment);

            File.WriteAllText(
                "split-segments.json",
                JsonConvert.SerializeObject(segments, Formatting.Indented, _serializerSettings));

            File.WriteAllText(
                "split-turns.json",
                JsonConvert.SerializeObject(turns, Formatting.Indented, _serializerSettings));

            File.WriteAllText($"{beforeSplit.Id}.gpx", beforeSplit.AsGpx());
            File.WriteAllText($"{afterSplit.Id}.gpx", afterSplit.AsGpx());
        }

        private void Dwim(
            List<SegmentTurns> turns, 
            SegmentTurn segmentToSplitTurnNode, 
            SegmentTurn targetSegmentTurnNode, 
            string originalSegmentId, 
            string replacementSegmentId)
        {
            if (segmentToSplitTurnNode.GoStraight != null)
            {
                targetSegmentTurnNode.GoStraight = segmentToSplitTurnNode.GoStraight;

                var targetSegmentTurn = turns.Single(t => t.SegmentId == segmentToSplitTurnNode.GoStraight);
                ReplaceTurnReferences(targetSegmentTurn, originalSegmentId, replacementSegmentId);
            }

            if (segmentToSplitTurnNode.Left != null)
            {
                targetSegmentTurnNode.Left = segmentToSplitTurnNode.Left;

                var targetSegmentTurn = turns.Single(t => t.SegmentId == segmentToSplitTurnNode.Left);
                ReplaceTurnReferences(targetSegmentTurn, originalSegmentId, replacementSegmentId);
            }

            if (segmentToSplitTurnNode.Right != null)
            {
                targetSegmentTurnNode.Right = segmentToSplitTurnNode.Right;

                var targetSegmentTurn = turns.Single(t => t.SegmentId == segmentToSplitTurnNode.Right);
                ReplaceTurnReferences(targetSegmentTurn, originalSegmentId, replacementSegmentId);
            }
        }

        private void ReplaceTurnReferences(SegmentTurns turn, string originalId, string replacementId)
        {
            ReplaceTurnNodeReferences(originalId, replacementId, turn.TurnsA);
            ReplaceTurnNodeReferences(originalId, replacementId, turn.TurnsB);
        }

        private static void ReplaceTurnNodeReferences(string originalId, string replacementId, SegmentTurn node)
        {
            if (originalId.Equals(node.GoStraight))
            {
                node.GoStraight = replacementId;
            }

            if (originalId.Equals(node.Left))
            {
                node.Left = replacementId;
            }

            if (originalId.Equals(node.Right))
            {
                node.Right = replacementId;
            }
        }
    }
}
