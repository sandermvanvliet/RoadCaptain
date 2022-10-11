using System;
using System.Collections.Generic;
using System.IO;

namespace RoadCaptain.Tests.Benchmark
{
    public class BinarySegmentSerializer
    {
        private static readonly char SegmentSeparator = (char)0xFF;

        public static void SerializeSegment(BinaryWriter writer, Segment segment)
        {
            writer.Write(segment.Id);
            writer.Write(segment.Name);
            writer.Write(segment.Sport.ToString());
            writer.Write(segment.NoSelectReason ?? string.Empty);

            foreach (var point in segment.Points)
            {
                writer.Write(point.Latitude);
                writer.Write(point.Longitude);
                writer.Write(point.Altitude);
                writer.Write(point.Index.Value);
                writer.Write(point.DistanceOnSegment);
                writer.Write(point.DistanceFromLast);
            }
            
            writer.Write(SegmentSeparator);
        }

        public static Segment DeserializeSegment(BinaryReader reader)
        {
            var segmentId = reader.ReadString();
            var segmentName = reader.ReadString();
            var sport = reader.ReadString();
            var noSelectReason = reader.ReadString();

            var points = new List<TrackPoint>();

            while (reader.BaseStream.Position < reader.BaseStream.Length - 2)
            {
                var latitude = reader.ReadDouble();
                var longitude = reader.ReadDouble();
                var altitude = reader.ReadDouble();
                var index = reader.ReadInt32();
                var distanceOnSegment = reader.ReadDouble();
                var distanceFromLast = reader.ReadDouble();

                points.Add(new TrackPoint(latitude,longitude,altitude) { DistanceFromLast = distanceFromLast, DistanceOnSegment = distanceOnSegment, Index = index });
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
    }
}