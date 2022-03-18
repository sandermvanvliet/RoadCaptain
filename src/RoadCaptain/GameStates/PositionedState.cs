using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class PositionedState : InGameState
    {
        [JsonProperty]
        public TrackPoint CurrentPosition { get; private set; }

        public PositionedState(uint riderId, ulong activityId, TrackPoint currentPosition)
            : base(riderId, activityId)
        {
            CurrentPosition = currentPosition;
        }
    }
}