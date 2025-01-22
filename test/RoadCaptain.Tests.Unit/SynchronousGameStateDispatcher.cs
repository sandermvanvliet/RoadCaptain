// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        private readonly List<Action<string>> _startRouteHandlers = new();
        
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

        public void LoggedIn()
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

        public void InvalidCredentials(Exception exception)
        {
            Dispatch(new InvalidCredentialsState(exception));
        }

        public void StartRoute()
        {
            _startRouteHandlers.ToList().ForEach(handler => handler(""));
        }

        public void IncorrectConnectionSecret()
        {
            throw new NotImplementedException();
        }

        public void Start(CancellationToken token)
        {
        }

        public void ReceiveRoute(Action<PlannedRoute> routeSelected)
        {
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
        }

        public void ReceiveLastSequenceNumber(Action<ulong> lastSequenceNumber)
        {
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
        }

        public void ReceiveGameState(Action<GameStates.GameState> gameState)
        {
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
