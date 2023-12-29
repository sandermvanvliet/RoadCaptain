// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.ZwiftRouteDownloader
{
    public class Segment
    {
        public long StravaSegmentId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string World { get; set; }
        public decimal Distance { get; set; }
        public string Type { get; set; }
        public string StravaSegmentUrl { get; set; }
    }
}
