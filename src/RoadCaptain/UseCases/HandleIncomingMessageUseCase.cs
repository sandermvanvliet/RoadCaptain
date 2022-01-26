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
            /* DESIGN NOTES:
             *
             * Because we know that we're receiving length-delimited Protobuf messages in a continuous byte
             * stream. That means that we can't simply read all data from the socket until it runs out because
             * the remaining bytes of a particular Protobuf message may yet arrive...
             * Therefore we'll go with the following approach:
             * - IMessageReceiver will return all bytes _it_ can currently read
             * - This use case will then:
             *   - assemble those into a stream
             *   - attempt to detect Protobuf message boundaries
             *   - split out the relevant bytes
             *   - attempt to parse a Protobuf message from those bytes
             *   - emit the message if that was successful on the IMessageEmitter
             * and then rinse & repeat until the cancellation token is flagged.
             *
             * That _should_ apply some logical splits in the right places, network stuff is in the IMessageReceiver
             * adapter and message spitting & parsing is in the use case. This should allow us to test properly
             * as we can simply create a IMessageReceiver that spits out bytes how we want them.
             */


            // do-while to at least attempt one receive action
            do
            {
                // Note: this call will block when using an actual network socket
                var bytes = _messageReceiver.ReceiveMessageBytes();

                if (bytes != null)
                {
                    _messageEmitter.Emit(bytes);
                }
            } while (!token.IsCancellationRequested);
        }
    }
}