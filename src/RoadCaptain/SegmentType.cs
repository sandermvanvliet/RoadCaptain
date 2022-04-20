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