using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class OnSegmentState : PositionedState
    {
        [JsonProperty]
        public Segment CurrentSegment { get; private set; }

        public OnSegmentState(ulong activityId, TrackPoint currentPosition, Segment segment) 
            : base(activityId, currentPosition)
        {
            CurrentSegment = segment;
        }
    }
}