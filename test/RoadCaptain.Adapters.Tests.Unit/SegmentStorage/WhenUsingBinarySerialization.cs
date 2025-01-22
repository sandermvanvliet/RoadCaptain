// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;

namespace RoadCaptain.Adapters.Tests.Unit.SegmentStorage
{
    public class WhenUsingBinarySerialization
    {
        [Fact]
        public void Roundtrip()
        {
            var segments = new List<Segment>
            {
                new(new List<TrackPoint> { new(1, 1, 1), new(1, 1, 2), new(1, 1, 3) }) { Id = "seg-1", Name = "Segment 1", Sport = SportType.Cycling },
                new(new List<TrackPoint> { new(2, 1, 1), new(2, 1, 2), new(2, 1, 3) }) { Id = "seg-2", Name = "Segment 2", Sport = SportType.Running },
                new(new List<TrackPoint> { new(3, 1, 1), new(3, 1, 2), new(3, 1, 3) }) { Id = "seg-3", Name = "Segment 3", Sport = SportType.Both }
            };

            var memoryStream = new MemoryStream();
            
            using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                BinarySegmentSerializer.SerializeSegments(writer, segments);
                writer.Flush();
            }

            // Rewind stream
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var reader = new BinaryReader(memoryStream);
            var deserializedSegments = BinarySegmentSerializer.DeserializeSegments(reader);

            deserializedSegments
                .Should()
                .BeEquivalentTo(segments);
        }

        [Fact]
        public void Production()
        {
            using var reader = new BinaryReader(File.OpenRead("segments-watopia.bin"));
            var deserializedSegments = BinarySegmentSerializer.DeserializeSegments(reader);
        }
    }
}