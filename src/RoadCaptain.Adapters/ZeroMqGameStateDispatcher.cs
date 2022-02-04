using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class ZeroMqGameStateDispatcher : InMemoryGameStateDispatcher
    {
        private readonly PublisherSocket _publishSocket;

        public ZeroMqGameStateDispatcher(MonitoringEvents monitoringEvents) : base(monitoringEvents)
        {
            _publishSocket = new PublisherSocket();
            _publishSocket.Bind("tcp://localhost:7001");
        }

        protected override void Enqueue(string topic, object data)
        {
            var message = new Message
            {
                TimeStamp = DateTime.UtcNow,
                Data = JsonConvert.SerializeObject(data)
            };

            var serializedContent = JsonConvert.SerializeObject(message);

            _publishSocket
                .SendMoreFrame(topic)
                .SendFrame(serializedContent);
        }
    }

    internal class Message
    {
        public string Version => "0.1";
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }
    }
}