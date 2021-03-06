// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class InMemoryGameStateDispatcher : IGameStateDispatcher, IGameStateReceiver
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

        public InMemoryGameStateDispatcher(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _queue = new ConcurrentQueue<Message>();
        }

        public ulong LastSequenceNumber { get; private set; }

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

        public void RouteSelected(PlannedRoute route)
        {
            Enqueue("routeSelected", route);
        }

        public void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            if (sequenceNumber > LastSequenceNumber)
            {
                LastSequenceNumber = sequenceNumber;
                Enqueue("lastSequenceNumber", sequenceNumber);
            }
        }

        public void Dispatch(GameState gameState)
        {
            Enqueue("gameState", gameState);
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

        public void Register(Action<PlannedRoute> routeSelected, Action<ulong> lastSequenceNumber, Action<GameState> gameState)
        {
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
            AddHandlerIfNotNull(_gameStateHandlers, gameState);
        }

        public void Drain()
        {
            while (_working)
            {

            }
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage> handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }

        private void InvokeHandlers(Message message)
        {
            if (message == null)
            {
                return;
            }

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
    }
}
