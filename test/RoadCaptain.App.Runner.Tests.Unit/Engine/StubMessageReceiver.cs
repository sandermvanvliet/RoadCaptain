using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class StubMessageReceiver : IMessageReceiver
    {
        public byte[]? ReceiveMessageBytes()
        {
            return null;
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Shutdown()
        {
        }
    }
}