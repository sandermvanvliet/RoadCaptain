using System;

namespace RoadCaptain.Ports
{
    public interface IMessageEmitter
    {
        void EmitMessageFromBytes(byte[] payload, long sequenceNumber);
    }
}