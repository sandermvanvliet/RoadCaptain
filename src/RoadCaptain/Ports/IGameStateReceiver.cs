using System;
using System.Threading;
using RoadCaptain.GameStates;

namespace RoadCaptain.Ports
{
    public interface IGameStateReceiver
    {
        void Start(CancellationToken token);
        void Register(Action<PlannedRoute> routeSelected,
            Action<ulong> lastSequenceNumber, Action<GameState> gameState);

        void Drain();
    }
}