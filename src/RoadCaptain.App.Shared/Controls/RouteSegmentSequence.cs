// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.Shared.Controls
{
    public class RouteSegmentSequence
    {
        public SegmentDirection Direction { get; set; }
        public string? SegmentId { get; set; }
        public SegmentSequenceType Type { get; set; }
        public bool IsLeadIn => Type == SegmentSequenceType.LeadIn;
    }
}
