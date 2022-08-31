using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoadCaptain.GameStates;
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

        public void LoggedIn(string zwiftAccessToken)
        {
            throw new NotImplementedException();
        }

        public void WaitingForConnection()
        {
            throw new NotImplementedException();
        }

        public void Connected()
        {
            throw new NotImplementedException();
        }

        public void EnterGame(uint riderId, ulong activityId)
        {
            throw new NotImplementedException();
        }

        public void LeaveGame()
        {
            throw new NotImplementedException();
        }

        public void UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw new NotImplementedException();
        }

        public void TurnCommandAvailable(string type)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception exception)
        {
            Dispatch(new ErrorState(exception));
        }

        public void Error(string message, Exception exception)
        {
            Dispatch(new ErrorState(message, exception));
        }

        public void Start(CancellationToken token)
        {
        }

        public void Register(Action<PlannedRoute>? routeSelected, Action<ulong>? lastSequenceNumber,
            Action<GameStates.GameState>? gameState)
        {
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
            AddHandlerIfNotNull(_gameStateHandlers, gameState);
        }

        public void Drain()
        {
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage>? handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }
    }
}