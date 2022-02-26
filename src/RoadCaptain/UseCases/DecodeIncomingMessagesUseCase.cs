using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class DecodeIncomingMessagesUseCase
    {
        /// <summary>
        /// Zwift uses a 2 byte length value to indicate the size of the message payload
        /// </summary>
        private const int MessageLengthPrefix = 2;
        private readonly IMessageReceiver _messageReceiver;
        private readonly IMessageEmitter _messageEmitter;
        private readonly MonitoringEvents _monitoringEvents;

        public DecodeIncomingMessagesUseCase(
            IMessageReceiver messageReceiver,
            IMessageEmitter messageEmitter, MonitoringEvents monitoringEvents)
        {
            _messageReceiver = messageReceiver;
            _messageEmitter = messageEmitter;
            _monitoringEvents = monitoringEvents;
        }

        public Task ExecuteAsync(CancellationToken token)
        {
            // do-while to at least attempt one receive action
            do
            {
                var bytes = _messageReceiver.ReceiveMessageBytes();

                if (bytes == null || bytes.Length <= 0)
                {
                    // Nothing to do, wrap around and expect ReceiveMessageBytes to block
                    continue;
                }

                var offset = 0;
                var readOnlySequence = new ReadOnlySequence<byte>(bytes);

                // Now that we have a sequence of bytes we need to detect
                // message lengths and then proceed to split the buffer
                // into the relevant actual payloads. 
                // The message emitter expects _only_ the payload so what
                // we'll do here is read 2 bytes (MessageLengthPrefix) and
                // from that take the length of the payload. Then we call
                // the emitter with the sliced payload and then proceed to
                // check if there is another message in bytes we've received.
                while (offset < bytes.Length)
                {
                    var payloadLength = ToUInt16(readOnlySequence, offset, MessageLengthPrefix);

                    var end = offset + MessageLengthPrefix + payloadLength;

                    if (end > readOnlySequence.Length)
                    {
                        _monitoringEvents.Warning(
                            "Buffer does not contain enough data. Expected {PayloadLength} but only have {DataLength} left",
                            payloadLength,
                            readOnlySequence.Length - offset - MessageLengthPrefix);

                        break;
                    }

                    var toSend = readOnlySequence.Slice(offset + MessageLengthPrefix, payloadLength);

                    // Note: A payload of 1 is invalid because that would mean there
                    //       is only a tag index + wire type which can't happen.
                    if (toSend.Length > 1)
                    {
                        _messageEmitter.EmitMessageFromBytes(toSend.ToArray());
                    }

                    offset += (MessageLengthPrefix + (int)toSend.Length);
                }
            } while (!token.IsCancellationRequested);

            return Task.CompletedTask;
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