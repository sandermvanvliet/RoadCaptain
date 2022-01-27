using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleIncomingMessageUseCase
    {
        /// <summary>
        /// Zwift uses a 2 byte length value to indicate the size of the message payload
        /// </summary>
        private const int MessageLengthPrefix = 2;
        private readonly IMessageReceiver _messageReceiver;
        private readonly IMessageEmitter _messageEmitter;
        private readonly Pipe _pipe;
        private readonly MonitoringEvents _monitoringEvents;

        public HandleIncomingMessageUseCase(
            IMessageReceiver messageReceiver,
            IMessageEmitter messageEmitter, MonitoringEvents monitoringEvents)
        {
            _messageReceiver = messageReceiver;
            _messageEmitter = messageEmitter;
            _monitoringEvents = monitoringEvents;
            _pipe = new Pipe();
        }

        public async Task ExecuteAsync(CancellationToken token)
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
            
            var processBytesTask = Task.Factory.StartNew(async () => await ProcessMessagesAsync(_pipe.Reader, token), TaskCreationOptions.LongRunning);

            // do-while to at least attempt one receive action
            do
            {
                // Note: this call will block when using an actual network socket
                var bytes = _messageReceiver.ReceiveMessageBytes();

                if (bytes != null && bytes.Length > 0)
                {
                    // TODO: Consider to supply this to ReceiveMessageBytes above to remove allocations
                    var mem = _pipe.Writer.GetMemory(bytes.Length);

                    // Ugly, but see ^^^
                    bytes.CopyTo(mem);
                    
                    _pipe.Writer.Advance(bytes.Length);
                    
                    // Tell the reader there are bytes available to consume
                    await _pipe.Writer.FlushAsync(token);
                }
            } while (!token.IsCancellationRequested);

            try
            {
                await processBytesTask;
            }
            catch (OperationCanceledException)
            {
                // Nop
            }
        }

        async Task ProcessMessagesAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await reader.ReadAsync(cancellationToken);
                    var buffer = result.Buffer;

                    try
                    {
                        // Process all messages from the buffer, modifying the input buffer on each
                        // iteration.
                        while (TryExtractMessage(ref buffer, out byte[] payload))
                        {
                            _monitoringEvents.ReceivedMessage();
                            _messageEmitter.Emit(payload);
                        }

                        // There's no more data to be processed.
                        if (result.IsCompleted)
                        {
                            if (buffer.Length > 0)
                            {
                                // The message is incomplete and there's no more data to process.
                                throw new InvalidDataException("Incomplete message.");
                            }
                            break;
                        }
                    }
                    finally
                    {
                        // Since all messages in the buffer are being processed, you can use the
                        // remaining buffer's Start and End position to determine consumed and examined.
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            finally
            {
                await reader.CompleteAsync();
            }
        }

        private static bool TryExtractMessage(ref ReadOnlySequence<byte> buffer, out byte[] payload)
        {
            var payloadLength = ToUInt16(buffer, 0, MessageLengthPrefix);

            if(buffer.Length - MessageLengthPrefix < payloadLength)
            {
                // Not enough bytes in the buffer for the message that we expecteds
                payload = default;
                return false;
            }

            payload = buffer.Slice(MessageLengthPrefix, payloadLength).ToArray();
            buffer = buffer.Slice(MessageLengthPrefix + payloadLength);

            return true;
        }
        
        private static int ToUInt16(ReadOnlySequence<byte> buffer, int start, int count)
        {
            if (buffer.Length >= start + count)
            {
                var b = buffer.Slice(start, count).ToArray();
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }

                if (b.Length == count)
                {
                    return (BitConverter.ToUInt16(b, 0));
                }

                return 0;
            }

            return 0;
        }
    }
}