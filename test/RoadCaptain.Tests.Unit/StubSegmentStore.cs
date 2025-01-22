// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class StubSegmentStore : ISegmentStore
    {
        public List<Segment> LoadSegments(World world, SportType sport)
        {
            return new List<Segment>();
        }

        public List<Segment> LoadMarkers(World world)
        {
            return new List<Segment>();
        }
    }
}
