// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using RoadCaptain.GameStates;

namespace RoadCaptain.Ports
{
    public interface IGameStateReceiver
    {
        void Start(CancellationToken token);
        void ReceiveRoute(Action<PlannedRoute> routeSelected);
        void ReceiveLastSequenceNumber(Action<ulong> lastSequenceNumber);
        void ReceiveGameState(Action<GameState> gameState);
        void Drain();
    }
}
