using System.Threading;

namespace RoadCaptain.Ports
{
    public interface IMessageEmitter
    {
        void EmitMessageFromBytes(byte[] payload);
        ZwiftMessage? Dequeue(CancellationToken token);
    }
}