using System;
using System.Collections.Generic;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    // ZeroMQ pub/sub: https://netmq.readthedocs.io/en/latest/pub-sub/
    internal class ZeroMqGameStateReceiver : IGameStateReceiver
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly SubscriberSocket _subscriberSocket;

        private readonly List<Action<SegmentDirection>> _directionChangedHandlers = new();
        private readonly List<Action<TrackPoint>> _positionChangedHandlers = new();
        private readonly List<Action<string>> _segmentChangedHandlers = new();
        private readonly List<Action<List<TurnDirection>>> _turnCommandsAvailableHandlers = new();
        private readonly List<Action<List<Turn>>> _turnsAvailableHandlers = new();

        public ZeroMqGameStateReceiver(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _subscriberSocket = new SubscriberSocket();
        }

        public void Start(CancellationToken token)
        {
            _subscriberSocket.Connect("tcp://localhost:7001");

            _subscriberSocket.SubscribeToAnyTopic();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var serializedContent = _subscriberSocket.ReceiveFrameString();

                    InvokeHandlers(serializedContent);
                }
            }
            finally
            {
                _subscriberSocket.Close();
            }
        }

        public void Register(
            Action<TrackPoint> positionChanged,
            Action<string> segmentChanged,
            Action<List<Turn>> turnsAvailable,
            Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable)
        {
            _positionChangedHandlers.Add(positionChanged);
            _segmentChangedHandlers.Add(segmentChanged);
            _turnsAvailableHandlers.Add(turnsAvailable);
            _directionChangedHandlers.Add(directionChanged);
            _turnCommandsAvailableHandlers.Add(turnCommandsAvailable);
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