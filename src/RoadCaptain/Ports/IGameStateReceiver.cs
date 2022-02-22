using System;
using System.Collections.Generic;
using System.Threading;
using RoadCaptain.GameStates;

namespace RoadCaptain.Ports
{
    public interface IGameStateReceiver
    {
        void Start(CancellationToken token);
        void Register(Action<TrackPoint> positionChanged,
            Action<string> segmentChanged,
            Action<List<Turn>> turnsAvailable,
            Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable,
            Action<ulong> enteredGame,
            Action<ulong> leftGame,
            Action<PlannedRoute> routeSelected,
            Action<uint> lastSequenceNumber, Action<GameState> gameState);

        void RegisterRouteEvents(
            Action routeStarted,
            Action<int> routeProgression,
            Action routeCompleted);
    }
}