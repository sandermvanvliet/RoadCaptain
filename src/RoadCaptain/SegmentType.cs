// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public enum SegmentType
    {
        Unknown,
        Segment, // RoadCaptain route segment
        Climb, // A KOM in Zwift
        Sprint, // A sprint in Zwift
        StravaSegment // A segment from Strava
    }
}
