// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public class ZwiftProfile
    {
        public long Id { get; set; }
        public string? PublicId { get; set; }
        public int? WorldId { get; set; }
        public bool Riding { get; set; }
        public bool LikelyInGame { get; set; }
    }
}
