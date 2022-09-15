using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    /// <summary>
    /// The connection secret used by Zwift to encrypt the messages is not what was sent on the initiate relay call
    /// </summary>
    /// <remarks>
    /// <para>This situation occurs when either Zwift is already running or when RoadCaptain restarts for some reason. In that case Zwift is using a connection secret that is different from what we expect and we need to initialize the relay again.</para>
    /// <para>All state transition calls will simply return this state again to prevent anything else from happening.</para>
    /// </remarks>
    public sealed class IncorrectConnectionSecretState : GameState
    {
        public override uint RiderId => 0;

        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            return this;
        }

        public override GameState LeaveGame()
        {
            return this;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            return this;
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}