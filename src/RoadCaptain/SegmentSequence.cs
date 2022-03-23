// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public class SegmentSequence
    {
        public string SegmentId { get; set; }
        public TurnDirection TurnToNextSegment { get; set; } = TurnDirection.None;
        public string NextSegmentId { get; set; }
        public SegmentDirection Direction { get; set; } = SegmentDirection.Unknown;
    }
}
