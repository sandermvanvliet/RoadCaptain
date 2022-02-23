using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class PositionedState : InGameState
    {
        [JsonProperty]
        public TrackPoint CurrentPosition { get; private set; }

        public PositionedState(ulong activityId, TrackPoint currentPosition)
            : base(activityId)
        {
            CurrentPosition = currentPosition;
        }
    }
}