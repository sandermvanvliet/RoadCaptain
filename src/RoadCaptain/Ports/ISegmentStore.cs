// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;

namespace RoadCaptain.Ports
{
    public interface ISegmentStore
    {
        List<Segment> LoadSegments(World world, SportType sport);
        List<Segment> LoadMarkers(World world);
    }
}

