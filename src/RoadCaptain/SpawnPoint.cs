// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public class SpawnPoint
    {
        public string SegmentId { get; set; }
        public string ZwiftRouteName { get; set; }
        public SegmentDirection Direction { get; set; }
        public SportType Sport { get; set; }
    }
}
