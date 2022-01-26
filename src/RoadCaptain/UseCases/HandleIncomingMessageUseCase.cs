using System.Threading;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleIncomingMessageUseCase
    {
        private readonly IMessageReceiver _messageReceiver;
        private readonly IMessageEmitter _messageEmitter;

        public HandleIncomingMessageUseCase(
            IMessageReceiver messageReceiver,
            IMessageEmitter messageEmitter)
        {
            _messageReceiver = messageReceiver;
            _messageEmitter = messageEmitter;
        }

        public void Execute(CancellationToken token)
        {
            var bytes = _messageReceiver.ReceiveMessageBytes();

            if (bytes != null)
            {
                _messageEmitter.Emit(bytes);
            }
        }
    }
}