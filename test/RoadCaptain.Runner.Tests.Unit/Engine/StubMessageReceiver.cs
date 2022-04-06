using RoadCaptain.Ports;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class StubMessageReceiver : IMessageReceiver
    {
        public byte[] ReceiveMessageBytes()
        {
            return null;
        }

        public void Shutdown()
        {
        }
    }
}