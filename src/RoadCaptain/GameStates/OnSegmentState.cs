namespace RoadCaptain.GameStates
{
    public class OnSegmentState : PositionedState
    {
        public Segment CurrentSegment { get; }

        public OnSegmentState(ulong activityId, TrackPoint currentPosition, Segment segment) 
            : base(activityId, currentPosition)
        {
            CurrentSegment = segment;
        }
    }
}