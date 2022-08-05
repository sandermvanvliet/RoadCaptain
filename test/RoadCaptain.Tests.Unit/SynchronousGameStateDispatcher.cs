using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    public class SynchronousGameStateDispatcher : IGameStateDispatcher, IGameStateReceiver
    {
        private readonly List<Action<PlannedRoute>> _routeSelectedHandlers = new();
        private readonly List<Action<ulong>> _lastSequenceNumberHandlers = new();
        private readonly List<Action<GameStates.GameState>> _gameStateHandlers = new();

        public void RouteSelected(PlannedRoute route)
        {
            _routeSelectedHandlers.ToList().ForEach(handler => handler(route));
        }

        public void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            _lastSequenceNumberHandlers.ToList().ForEach(handler => handler(sequenceNumber));
        }

        public void Dispatch(GameStates.GameState gameState)
        {
            _gameStateHandlers.ToList().ForEach(handler => handler(gameState));
        }

        public void Start(CancellationToken token)
        {
        }

        public void Register(Action<PlannedRoute> routeSelected, Action<ulong> lastSequenceNumber, Action<GameStates.GameState> gameState)
        {
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
            AddHandlerIfNotNull(_gameStateHandlers, gameState);
        }

        public void Drain()
        {
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage> handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }
    }
}