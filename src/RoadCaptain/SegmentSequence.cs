// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Newtonsoft.Json;

namespace RoadCaptain
{
    public class SegmentSequence
    {
        [JsonConstructor]
        public SegmentSequence(string segmentId, string? nextSegmentId, SegmentDirection direction, TurnDirection turnToNextSegment, SegmentSequenceType type)
        {
            SegmentId = segmentId;
            NextSegmentId = nextSegmentId;
            Direction = direction;
            TurnToNextSegment = turnToNextSegment;
            Type = type;
        }

        public SegmentSequence(string segmentId, SegmentDirection direction, SegmentSequenceType type)
        {
            SegmentId = segmentId;
            Direction = direction;
            Type = type;
        }

        public SegmentSequence(string segmentId, SegmentSequenceType type)
        {
            SegmentId = segmentId;
            Type = type;
        }

        public SegmentSequence(string segmentId, string? nextSegmentId, SegmentDirection direction, TurnDirection turnToNextSegment)
        {
            SegmentId = segmentId;
            NextSegmentId = nextSegmentId;
            Direction = direction;
            TurnToNextSegment = turnToNextSegment;
        }

        public SegmentSequence(string segmentId, SegmentSequenceType type, SegmentDirection direction, int index)
        {
            SegmentId = segmentId;
            Type = type;
            Direction = direction;
            Index = index;
        }

        public SegmentSequence(string segmentId, SegmentDirection direction)
        {
            SegmentId = segmentId;
            Direction = direction;
        }

        public SegmentSequence(string segmentId, string? nextSegmentId)
        {
            SegmentId = segmentId;
            NextSegmentId = nextSegmentId;
        }

        public SegmentSequence(string segmentId)
        {
            SegmentId = segmentId;
        }

        public string SegmentId { get; set; }
        public TurnDirection TurnToNextSegment { get; set; } = TurnDirection.None;
        public string? NextSegmentId { get; set; }
        public SegmentDirection Direction { get; set; } = SegmentDirection.Unknown;
        public SegmentSequenceType Type { get; set; } = SegmentSequenceType.Unknown;
        public int Index { get; set; }
    }
}
