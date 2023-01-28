// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class InMemoryGameStateDispatcher : IGameStateDispatcher, IGameStateReceiver, IDisposable
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ConcurrentQueue<Message> _queue;
        
        private readonly List<Action<PlannedRoute>> _routeSelectedHandlers = new();
        private readonly List<Action<ulong>> _lastSequenceNumberHandlers = new();
        private readonly List<Action<GameState>> _gameStateHandlers = new();
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly TimeSpan _queueWaitTimeout = TimeSpan.FromMilliseconds(2000);
        private bool _started;
        private static readonly object SyncRoot = new();
        private bool _working;

        private ulong _lastSequenceNumber;
        private GameState? _gameState;
        private StreamWriter? _output;
        private readonly IPathProvider _pathProvider;

        public InMemoryGameStateDispatcher(MonitoringEvents monitoringEvents, IPathProvider pathProvider)
        {
            _monitoringEvents = monitoringEvents;
            _pathProvider = pathProvider;
            _queue = new ConcurrentQueue<Message>();
        }

        private bool Started
        {
            get => _started;
            set
            {
                lock (SyncRoot)
                {
                    _started = value;
                }
            }
        }

        private GameState? State
        {
            get => _gameState;
            set
            {
                if (ReferenceEquals(_gameState, value))
                {
                    return;
                }

                // If no state is provided then default to the starting state
                value ??= new NotLoggedInState();

                if (_gameState != null && _gameState.GetType() != value.GetType())
                {
                    _monitoringEvents.Information("Game state changed from {OldState} to {NewState}", _gameState.GetType().Name, value.GetType().Name);
                }
                
                _gameState = value;

                Dispatch(_gameState);
            }
        }

        public void RouteSelected(PlannedRoute route)
        {
            Enqueue("routeSelected", route);
        }

        public void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            if (sequenceNumber > _lastSequenceNumber)
            {
                _lastSequenceNumber = sequenceNumber;
                Enqueue("lastSequenceNumber", sequenceNumber);
            }
        }

        public void Dispatch(GameState gameState)
        {
            Enqueue("gameState", gameState);
        }

        public void LoggedIn()
        {
            if (State is null or NotLoggedInState)
            {
                // Can only transition to LoggedInState from not-logged in state
                State = new LoggedInState();
            }
            else if (State is ConnectedToZwiftState or WaitingForConnectionState)
            {
                // Keep the existing state because these two states have valid credentials already
            }
            else
            {
                throw new InvalidStateTransitionException(
                    $"Can only transition to {nameof(LoggedInState)} from empty or not logged in state");
            }
        }

        public void WaitingForConnection()
        {
            if (State is not ReadyToGoState and not LoggedInState and not ConnectedToZwiftState and not IncorrectConnectionSecretState)
            {
                throw new InvalidStateTransitionException($"Can only transition to {nameof(WaitingForConnectionState)} state from {nameof(LoggedInState)} or {nameof(ConnectedToZwiftState)}");
            }

            State = new WaitingForConnectionState();
        }

        public void Connected()
        {
            if (State is not WaitingForConnectionState and not IncorrectConnectionSecretState)
            {
                throw new InvalidStateTransitionException($"Can only transition to {nameof(ConnectedToZwiftState)} state from {nameof(WaitingForConnectionState)}");
            }

            State = new ConnectedToZwiftState();
        }

        public void EnterGame(uint riderId, ulong activityId)
        {
            State = State?.EnterGame(riderId, activityId);

            InitializePositionLog();
        }

        private void InitializePositionLog()
        {
            try
            {
                var outputFileName = Path.Combine(_pathProvider.GetUserDataDirectory(),
                    $"positions-{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.log");
                _output = new StreamWriter(File.Open(
                        outputFileName,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.Read),
                    Encoding.UTF8)
                {
                    AutoFlush = true
                };
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to initialize position log file");
            }
        }

        public void LeaveGame()
        {
            if (GameState.IsInGame(State))
            {
                State = State?.LeaveGame();
            }

            _output?.Flush();
            _output?.Close();
            _output = null;
        }

        public void UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            LogPosition(position);
            State = State?.UpdatePosition(position, segments, plannedRoute);
        }

        private void LogPosition(TrackPoint position)
        {
            if(_output != null && _output.BaseStream.CanWrite)
            {
                var serialized = JsonConvert.SerializeObject(new { Timestamp = DateTime.Now, position.Latitude, position.Longitude, position.CoordinatesDecimal}, Formatting.None);
                _output.WriteLine(serialized);
            }
        }

        public void TurnCommandAvailable(string type)
        {
            if (State is OnRouteState or UpcomingTurnState)
            {
                State = State?.TurnCommandAvailable(type);
            }
        }

        public void Error(Exception exception)
        {
            State = new ErrorState(exception);
        }

        public void Error(string message, Exception exception)
        {
            State = new ErrorState(message, exception);
        }

        public void InvalidCredentials(Exception exception)
        {
            State = new InvalidCredentialsState(exception);
        }

        public void StartRoute()
        {
            if (State is not LoggedInState and not ConnectedToZwiftState)
            {
                throw new InvalidStateTransitionException($"Can only transition to {nameof(ReadyToGoState)} state from {nameof(LoggedInState)}");
            }

            State = new ReadyToGoState();
        }

        public void IncorrectConnectionSecret()
        {
            State = new IncorrectConnectionSecretState();
        }

        protected virtual void Enqueue(string topic, object data)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    TimeStamp = DateTime.UtcNow,
                    Data = data
                };

                _queue.Enqueue(message);
            }
            catch (Exception ex)
            {
                _monitoringEvents.Error(ex, "Failed to dispatch game state update");
            }
            finally
            {
                // Unblock the Dequeue method
                _autoResetEvent.Set();
            }
        }

        public void Start(CancellationToken token)
        {
            // Only have one running at a time,
            // it's possible that this call is 
            // made from various consumers and
            // that would cause threading issues
            // because we'd be dequeuing messages
            // from multiple threads.
            if (Started)
            {
                return;
            }

            Started = true;

            // To ensure that we don't block a long time 
            // when there are no items in the queue we
            // need to trigger the auto reset event when
            // the token is cancelled.
            token.Register(() => _autoResetEvent.Set());

            try
            {
                do
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        _working = true;

                        InvokeHandlers(message);
                    }
                    else
                    {
                        _working = false;

                        // Only wait if nothing is in the queue,
                        // otherwise loop around and take the next
                        // item from the queue without waiting.
                        _autoResetEvent.WaitOne(_queueWaitTimeout);
                    }
                } while (!token.IsCancellationRequested);
            }
            finally
            {
                Started = false;
            }
        }

        public void ReceiveRoute(Action<PlannedRoute> routeSelected)
        {
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
        }

        public void ReceiveLastSequenceNumber(Action<ulong> lastSequenceNumber)
        {
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
        }

        public void ReceiveGameState(Action<GameState> gameState)
        {
            AddHandlerIfNotNull(_gameStateHandlers, gameState);
        }

        public void Drain()
        {
            // Note: This is a fairly ugly approach to ensure that the queue of
            //       states waiting to be dispatched is cleared before stopping
            //       the dispatcher. It is (currently) only used for tests.
            while (_working)
            {
                Thread.Sleep(5);
            }
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage>? handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }

        private void InvokeHandlers(Message message)
        {
            switch (message.Topic)
            {
                case "routeSelected":
                    _routeSelectedHandlers.ToList().ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "lastSequenceNumber":
                    _lastSequenceNumberHandlers.ToList().ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "gameState":
                    _gameStateHandlers.ToList().ForEach(h => InvokeHandler(h, message.Data));
                    break;
            }
        }

        private void InvokeHandler<TMessage>(Action<TMessage> handle, object payload)
        {
            try
            {
                handle.DynamicInvoke(payload);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to invoke handler");
            }
        }

        public void Dispose()
        {
            _autoResetEvent.Dispose();
            _output?.Dispose();
        }
    }
}
