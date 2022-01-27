using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleIncomingMessageUseCase
    {
        private readonly IMessageReceiver _messageReceiver;
        private readonly IMessageEmitter _messageEmitter;
        private readonly Pipe _pipe;
        private readonly PipeWriter _writer;
        
        public HandleIncomingMessageUseCase(
            IMessageReceiver messageReceiver,
            IMessageEmitter messageEmitter)
        {
            _messageReceiver = messageReceiver;
            _messageEmitter = messageEmitter;
            _pipe = new Pipe();
            _writer = _pipe.Writer;
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
                    var mem = _writer.GetMemory(bytes.Length);

                    // Ugly, but see ^^^
                    bytes.CopyTo(mem);
                    
                    _writer.Advance(bytes.Length);
                    
                    // Tell the reader there are bytes available to consume
                    await _writer.FlushAsync(token);
                }
            } while (!token.IsCancellationRequested);
            
            await processBytesTask;
        }

        async Task ProcessMessagesAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadResult result = await reader.ReadAsync(cancellationToken);
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    try
                    {
                        // Process all messages from the buffer, modifying the input buffer on each
                        // iteration.
                        while (TryExtractMessage(ref buffer, out object message))
                        {
                            _messageEmitter.Emit(message);
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

        private static bool TryExtractMessage(ref ReadOnlySequence<byte> buffer, out object message)
        {
            var payloadLength = ToUInt16(buffer.Slice(0, 2).ToArray(), 0, 2);

            if(buffer.Length - 2 < payloadLength)
            {
                // Not enough bytes in the buffer for the message that we expecteds
                message = null;
                return false;
            }

            message = buffer.Slice(2, payloadLength).ToArray();
            buffer = buffer.Slice(2 + payloadLength);

            return true;
        }
        
        private static int ToUInt16(byte[] buffer, int start, int count)
        {
            if (buffer.Length >= start + count)
            {
                var b = buffer.Skip(start).Take(count).ToArray();
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