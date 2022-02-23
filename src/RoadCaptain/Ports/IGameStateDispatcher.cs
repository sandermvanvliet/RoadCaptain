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
        /// The rider has selected a route to follow
        /// </summary>
        /// <param name="route"></param>
        void RouteSelected(PlannedRoute route);

        /// <summary>
        /// Update the last known incoming message sequence number
        /// </summary>
        /// <param name="sequenceNumber"></param>
        void UpdateLastSequenceNumber(ulong sequenceNumber);

        /// <summary>
        /// The state of the game has changed
        /// </summary>
        /// <param name="gameState"></param>
        void Dispatch(GameState gameState);
    }
}