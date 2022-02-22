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
        /// The rider entered a segment and there are options to turn onto another segment
        /// </summary>
        /// <param name="turns">The available turns (direction + next segment)</param>
        /// <remarks>If the current segment is a direct connection to only one segment this is not called</remarks>
        void TurnsAvailable(List<Turn> turns);

        /// <summary>
        /// The direction of the rider on the segment has changed
        /// </summary>
        /// <param name="direction">The direction the rider is heading in</param>
        /// <remarks>This happens when a u-turn is executed. Otherwise it should not occcur much</remarks>
        void DirectionChanged(SegmentDirection direction);

        /// <summary>
        /// The rider is coming up to the end of a segment and the game has presented turn options
        /// </summary>
        /// <param name="turns">The directions in which turns can be made</param>
        /// <remarks>The turns received here _should_ correspond to what has been provided in <see cref="TurnsAvailable"/>.</remarks>
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
        bool InGame { get; }

        /// <summary>
        /// The rider started the planned route
        /// </summary>
        void RouteStarted();

        /// <summary>
        /// The rider entered the next segment of the planned route
        /// </summary>
        /// <param name="step"></param>
        /// <param name="segmentId"></param>
        void RouteProgression(int step, string segmentId);

        /// <summary>
        /// The rider ocmpleted the planned route
        /// </summary>
        void RouteCompleted();

        void Dispatch(GameState gameState);
    }
}