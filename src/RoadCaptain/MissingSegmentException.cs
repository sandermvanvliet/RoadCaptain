// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public class MissingSegmentException : Exception
    {
        public MissingSegmentException(string segmentId)
            : base("A segment of the route does not exist in this world")
        {
            Data.Add(nameof(segmentId), segmentId);
        }
    }
}
