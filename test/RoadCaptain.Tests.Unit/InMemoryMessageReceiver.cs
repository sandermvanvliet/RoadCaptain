using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageReceiver : IMessageReceiver
    {
        public byte[] ReceiveMessageBytes()
        {
            return AvailableBytes;
        }

        public void Shutdown()
        {
        }

        public byte[] AvailableBytes { get; set; }
    }
}