using System;
using System.Collections.Generic;
using System.Threading;
using RoadCaptain.Adapters;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageEmitter : IMessageEmitter
    {
        public InMemoryMessageEmitter()
        {
            Messages = new List<byte[]>();
            _autoResetEvent = new AutoResetEvent(false);
        }

        public List<byte[]> Messages { get; }
        private readonly Queue<ZwiftMessage> _messagesToEmit = new();
        private readonly AutoResetEvent _autoResetEvent;

        public void EmitMessageFromBytes(byte[] payload, long sequenceNumber)
        {
            Messages.Add(payload);
        }

        public ZwiftMessage Dequeue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_messagesToEmit.TryDequeue(out var message))
                {
                    return message;
                }

                _autoResetEvent.WaitOne(10);
            }

            return null;
        }

        public void Enqueue(ZwiftMessage message)
        {
            _messagesToEmit.Enqueue(message);
            _autoResetEvent.Set();
        }

        public void SubscribeOnPing(Action<int> callback)
        {
        }
    }
}