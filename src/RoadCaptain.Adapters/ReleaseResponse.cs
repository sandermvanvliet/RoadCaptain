// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class ReleaseResponse
    {
        public string? Id { get; set; }
        public string? TagName { get; set; }
        public string? Body { get; set; }
        public ReleaseAsset[]? Assets { get; set; }
        [JsonProperty("prerelease")]
        public bool PreRelease { get; set; }
        public bool Draft { get; set; }
        public DateTime CreatedAt { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
