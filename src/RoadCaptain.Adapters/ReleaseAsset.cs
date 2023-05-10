// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Adapters
{
    internal class ReleaseAsset
    {
        public string? Id { get; set; }
        public string? Url { get; set; }
        public string? Name { get; set; }
        public string? ContentType { get; set; }
        public string? BrowserDownloadUrl { get; set; }
    }
}
