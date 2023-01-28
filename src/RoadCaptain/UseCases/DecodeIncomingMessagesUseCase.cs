// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        private readonly IZwiftCrypto _zwiftCrypto;
        private readonly IGameStateDispatcher _dispatcher;
        private DateTime? _lastDataReceived;
        private Timer? _watchdogTimer;

        public DecodeIncomingMessagesUseCase(
            IMessageReceiver messageReceiver,
            IMessageEmitter messageEmitter,
            MonitoringEvents monitoringEvents,
            IZwiftCrypto zwiftCrypto,
            IGameStateDispatcher dispatcher)
        {
            _messageReceiver = messageReceiver;
            _messageEmitter = messageEmitter;
            _monitoringEvents = monitoringEvents;
            _zwiftCrypto = zwiftCrypto;
            _dispatcher = dispatcher;
        }

        public Task ExecuteAsync(CancellationToken token)
        {
            // Because socket.Accept() is a blocking call with
            // no way of setting a time-out or passing a cancellation 
            // token we need to be a bit clever.
            // Register a handler on the cancellation token which
            // effectively calls Shutdown() which calls Socket.Close()
            // which in turn ensures that Accept() is terminated.
            token.Register(() =>
            {
                _watchdogTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _watchdogTimer?.Dispose();
                _watchdogTimer = null;
                _messageReceiver.Shutdown();
            });

            // Check every 5 seconds, starting 5 seconds from now
            _watchdogTimer = new Timer(_ => DataReceivedWatchdog());
            _watchdogTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            // do-while to at least attempt one receive action
            do
            {
                var bytes = _messageReceiver.ReceiveMessageBytes();

                if (bytes == null || bytes.Length <= 0)
                {
                    // Nothing to do, wrap around and expect ReceiveMessageBytes to block
                    continue;
                }

                _lastDataReceived = DateTime.UtcNow;

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
                        var messageBytes = toSend.ToArray();

                        try
                        {
                            if (_zwiftCrypto != null)
                            {
                                var decryptionResult = _zwiftCrypto.Decrypt(messageBytes);

                                switch (decryptionResult)
                                {
                                    case SuccessfulDecryptionResult success:
                                        messageBytes = success.Data;
                                        // Have this inside the try/catch so that when decryption fails
                                        // this doesn't attempt to emit encrypted message bytes because
                                        // that will just fail down the line.
                                        _messageEmitter.EmitMessageFromBytes(messageBytes);
                                        break;
                                    case DecryptionFailedResult { Exception: CryptographyException cx }:
                                        _monitoringEvents.Error(cx, "Failed to decrypt message because of a cryptography error, further Zwift messages can't be decrypted");
                                        _dispatcher.IncorrectConnectionSecret();
                                        break;
                                    case DecryptionFailedResult { Exception: { } } failure:
                                        _monitoringEvents.Error(failure.Exception, "Failed to decrypt message because {Reason}, ignoring this message", failure.Exception.Message);
                                        break;
                                    case DecryptionFailedResult failure:
                                        _monitoringEvents.Error("Failed to decrypt message because {Reason}, ignoring this message", failure.Reason);
                                        break;
                                    default:
                                        throw new Exception("Got an unexpected result from Decrypt call that I don't know how to handle");
                                }
                            }
                            else
                            {
                                // If no IZwiftCrypto is provided then no decryption needs to happen
                                _messageEmitter.EmitMessageFromBytes(messageBytes);
                            }
                        }
                        catch (Exception e)
                        {
                            _monitoringEvents.Error(e, "Failed to decrypt message because {Reason}, ignoring this message", e.Message);
                        }
                    }

                    offset += MessageLengthPrefix + (int)toSend.Length;
                }
            } while (!token.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void DataReceivedWatchdog()
        {
            if (_lastDataReceived == null || DateTime.UtcNow.Subtract(_lastDataReceived.Value).TotalSeconds > 10)
            {
                _monitoringEvents.Debug("Did not receive any data in the last 10 seconds");
            }
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
