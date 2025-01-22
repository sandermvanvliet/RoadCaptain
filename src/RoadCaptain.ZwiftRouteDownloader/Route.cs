// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.ZwiftRouteDownloader
{
    public class Route
    {
        public long? StravaSegmentId { get; set; }
        public string Name => Slug;
        public string Slug { get; set; }
        public string World => WhatsOnZwiftUrl?.Replace("https://whatsonzwift.com/world/", "").Split("/")[0];
        public string StravaSegmentUrl { get; set; }
        public string[] Sports { get; set; } = { "Cycling" };
        public string WhatsOnZwiftUrl { get; set; }
    }
}
