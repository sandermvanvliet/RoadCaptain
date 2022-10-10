// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Adapters
{
    internal class ReleaseResponse
    {
        public string Id { get; set; }
        public string TagName { get; set; }
        public string Body { get; set; }
        public ReleaseAsset[] Assets { get; set; }
    }
}
