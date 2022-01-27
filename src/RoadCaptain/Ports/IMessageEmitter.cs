using System;
using System.Threading;

namespace RoadCaptain.Ports
{
    public interface IMessageEmitter
    {
        void EmitMessageFromBytes(byte[] payload, long sequenceNumber);
        ZwiftMessage Dequeue(CancellationToken token);
    }
}