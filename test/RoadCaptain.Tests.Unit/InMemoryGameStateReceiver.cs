using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    public class InMemoryGameStateReceiver : IGameStateReceiver
    {
        private readonly List<Action<SegmentDirection>> _directionChangedHandlers = new();
        private readonly List<Action<TrackPoint>> _positionChangedHandlers = new();
        private readonly List<Action<string>> _segmentChangedHandlers = new();
        private readonly List<Action<List<TurnDirection>>> _turnCommandsAvailableHandlers = new();
        private readonly List<Action<List<Turn>>> _turnsAvailableHandlers = new();
        private readonly List<Action<ulong>> _enteredGameHandlers = new();
        private readonly List<Action<ulong>> _leftGameHandlers = new();
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ConcurrentQueue<string> _messages = new();
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly List<Action<PlannedRoute>> _routeSelectedHandlers = new();

        public InMemoryGameStateReceiver(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }

        public void Start(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_messages.TryDequeue(out var serializedContent))
                {
                    InvokeHandlers(serializedContent);
                }

                _autoResetEvent.WaitOne(10);
            }
        }

        public void Register(Action<TrackPoint> positionChanged,
            Action<string> segmentChanged,
            Action<List<Turn>> turnsAvailable,
            Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable,
            Action<ulong> enteredGame,
            Action<ulong> leftGame, Action<PlannedRoute> routeSelected)
        {
            AddHandlerIfNotNull(_positionChangedHandlers, positionChanged);
            AddHandlerIfNotNull(_segmentChangedHandlers, segmentChanged);
            AddHandlerIfNotNull(_turnsAvailableHandlers, turnsAvailable);
            AddHandlerIfNotNull(_directionChangedHandlers, directionChanged);
            AddHandlerIfNotNull(_turnCommandsAvailableHandlers, turnCommandsAvailable);
            AddHandlerIfNotNull(_enteredGameHandlers, enteredGame);
            AddHandlerIfNotNull(_leftGameHandlers, leftGame);
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
        }
        
        public void Enqueue(string topic, object data)
        {
            _messages.Enqueue(JsonConvert.SerializeObject(new Message { Topic = topic, Data = JsonConvert.SerializeObject(data)}));
            _autoResetEvent.Set();
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage> handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }

        private void InvokeHandlers(string serializedContent)
        {
            var message = JsonConvert.DeserializeObject<Message>(serializedContent);
            if (message == null)
            {
                return;
            }

            switch (message.Topic)
            {
                case "positionChanged":
                    _positionChangedHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "segmentChanged":
                    _segmentChangedHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "turnsAvailable":
                    _turnsAvailableHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "directionChanged":
                    _directionChangedHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "turnCommandsAvailable":
                    _turnCommandsAvailableHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "enteredGame":
                    _enteredGameHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "leftGame":
                    _leftGameHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
                case "routeSelected":
                    _routeSelectedHandlers.ForEach(h => InvokeHandler(h, message.Data));
                    break;
            }
        }

        private void InvokeHandler<TMessage>(Action<TMessage> handle, string serializedContent)
        {
            try
            {
                handle(JsonConvert.DeserializeObject<TMessage>(serializedContent));
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to invoke handler");
            }
        }
    }
}