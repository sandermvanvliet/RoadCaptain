// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;

namespace RoadCaptain.ZwiftRouteDownloader
{
    public class StravaSegment
    {
        public List<decimal[]> LatLng { get; set; }
        public List<decimal> Altitude { get; set; }
    }
}
