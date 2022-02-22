using System.Collections.Generic;
using RoadCaptain.GameStates;

namespace RoadCaptain.Ports
{
    /*
     * Use this either as a singleton in-memory approach or implement this on both the sending and consuming sides of a queue.
     *
     * The use cases calling this are responsible for detecting the changes. Implementations of IGameStateDispatcher should not
     * have to perform debouncing.
     */
    public interface IGameStateDispatcher
    {
        /// <summary>
        /// The rider is coming up to the end of a segment and the game has presented turn options
        /// </summary>
        /// <param name="turns">The directions in which turns can be made</param>
        void TurnCommandsAvailable(List<TurnDirection> turns);

        /// <summary>
        /// The rider has selected a route to follow
        /// </summary>
        /// <param name="route"></param>
        void RouteSelected(PlannedRoute route);

        /// <summary>
        /// Update the last known incoming message sequence number
        /// </summary>
        /// <param name="sequenceNumber"></param>
        void UpdateLastSequenceNumber(ulong sequenceNumber);

        // TODO: Make this architectually sound
        // Because exposing the state like this is a bit ugly...
        List<TurnDirection> AvailableTurnCommands { get; }
        Segment CurrentSegment { get; }

        void Dispatch(GameState gameState);
    }
}