using System;
using System.Collections.Generic;
using System.Threading;
using RoadCaptain.GameStates;

namespace RoadCaptain.Ports
{
    public interface IGameStateReceiver
    {
        void Start(CancellationToken token);
        void Register(Action<List<Turn>> turnsAvailable,
            Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable,
            Action<PlannedRoute> routeSelected,
            Action<ulong> lastSequenceNumber, Action<GameState> gameState);
    }
}