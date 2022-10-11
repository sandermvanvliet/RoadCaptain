// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;

namespace RoadCaptain.Adapters
{
    internal class BinarySegmentSerializer
    {
        private static void SerializeSegment(BinaryWriter writer, Segment segment)
        {
            // Write the segment metadata
            writer.Write(segment.Id);
            writer.Write(segment.Name);
            writer.Write(segment.Sport.ToString());
            writer.Write(segment.NoSelectReason ?? string.Empty);

            // Write the number of points so that we don't have to look
            // for a delimiter which is always a bit error prone.
            writer.Write(segment.Points.Count);

            // Write all the points
            foreach (var point in segment.Points)
            {
                writer.Write(point.Latitude);
                writer.Write(point.Longitude);
                writer.Write(point.Altitude);
                writer.Write(point.Index.GetValueOrDefault(0));
                writer.Write(point.DistanceOnSegment);
                writer.Write(point.DistanceFromLast);
            }
        }

        private static Segment DeserializeSegment(BinaryReader reader)
        {
            var segmentId = reader.ReadString();
            var segmentName = reader.ReadString();
            var sport = reader.ReadString();
            var noSelectReason = reader.ReadString();

            if (noSelectReason == string.Empty)
            {
                noSelectReason = null;
            }

            var pointCount = reader.ReadInt32();

            var points = new List<TrackPoint>(pointCount);

            for (var i = 0; i < pointCount; i++)
            {
                var latitude = reader.ReadDouble();
                var longitude = reader.ReadDouble();
                var altitude = reader.ReadDouble();
                var index = reader.ReadInt32();
                var distanceOnSegment = reader.ReadDouble();
                var distanceFromLast = reader.ReadDouble();

                points.Add(new TrackPoint(latitude, longitude, altitude)
                {
                    DistanceFromLast = distanceFromLast, DistanceOnSegment = distanceOnSegment, Index = index
                });
            }

            return new Segment(points)
            {
                Id = segmentId,
                Name = segmentName,
                NoSelectReason = noSelectReason,
                Sport = Enum.Parse<SportType>(sport),
                Type = SegmentType.Segment
            };
        }

        public static void SerializeSegments(BinaryWriter writer, List<Segment> segments)
        {
            foreach (var segment in segments)
            {
                SerializeSegment(writer, segment);
            }
        }

        public static List<Segment> DeserializeSegments(BinaryReader reader)
        {
            var segments = new List<Segment>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                segments.Add(DeserializeSegment(reader));
            }

            return segments;
        }
    }
}