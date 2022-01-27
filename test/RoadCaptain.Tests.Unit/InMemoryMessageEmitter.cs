using System;
using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageEmitter : IMessageEmitter
    {
        public InMemoryMessageEmitter()
        {
            Messages = new List<byte[]>();
        }

        public List<byte[]> Messages { get; }

        public void EmitMessageFromBytes(byte[] payload, long sequenceNumber)
        {
            Messages.Add(payload);
        }

        public void SubscribeOnPing(Action<int> callback)
        {
        }
    }
}