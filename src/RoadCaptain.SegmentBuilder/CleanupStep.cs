// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.SegmentBuilder
{
    internal class CleanupStep
    {
        public static void Run(List<Segment> segments)
        {
            // Remove very short segments
            var toRemove = segments
                .Where(s => s.Distance < 20)
                .ToList();

            foreach (var segment in toRemove)
            {
                segments.Remove(segment);
            }
        }
    }
}
