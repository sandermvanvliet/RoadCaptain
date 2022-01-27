using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageEmitterToQueue : IMessageEmitter
    {
        public void EmitMessageFromBytes(byte[] payload, long messageSequenceNumber)
        {
        }
    }
}