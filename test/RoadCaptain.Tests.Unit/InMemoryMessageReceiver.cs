using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageReceiver : IMessageReceiver
    {
        public byte[] ReceiveMessageBytes()
        {
            return AvailableBytes;
        }

        public byte[] AvailableBytes { get; set; }
    }
}