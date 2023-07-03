// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public class RouteModel
    {
        public long Id { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorZwiftProfileId { get; set; }
        public string? Name { get; set; }
        public string? ZwiftRouteName { get; set; }
        public decimal Distance { get; set; }
        public decimal Ascent { get; set; }
        public decimal Descent { get; set; }
        public bool IsLoop { get; set; }
        public string? Serialized { get; set; }
        public string? RepositoryName { get; set; }
        public Uri? Uri { get; set; }
        public PlannedRoute? PlannedRoute { get; set; }
        public string? World { get; set; }
    }
}
