// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WheDecodingMessages
    {
        private readonly InMemoryMessageReceiver _messageReceiver;
        private readonly DecodeIncomingMessagesUseCase _useCase;
        private readonly InMemoryMessageEmitter _messageEmitter;
        private readonly ControllableZwiftCrypto _controllableZwiftCrypto;

        public WheDecodingMessages()
        {
            _messageReceiver = new InMemoryMessageReceiver();
            _messageEmitter = new InMemoryMessageEmitter();
            _controllableZwiftCrypto = new ControllableZwiftCrypto();
            _useCase = new DecodeIncomingMessagesUseCase(_messageReceiver, _messageEmitter, new NopMonitoringEvents(), _controllableZwiftCrypto, new NopGameStateDispatcher());
        }

        [Fact]
        public void GivenNoBytesAvailable_NoMessageIsEmitted()
        {
            WhenReceivingMessage();

            _messageEmitter
                .Messages
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenBytesForExactlyOneMessage_MessageIsEmitted()
        {
            GivenBytesOnNetwork(new byte[] { 0x2, 0x3, 0x4 });

            WhenReceivingMessage();

            _messageEmitter
                .Messages
                .Single()
                .Should()
                .BeEquivalentTo( new byte[] {0x2, 0x3, 0x4 });
        }

        [Fact]
        public void GivenBytesForTwoMessages_TwoMessageAreEmitted()
        {
            GivenBytesOnNetwork(new byte[] { 0x2, 0x3, 0x4 }, new byte[] { 0x5, 0x6});

            WhenReceivingMessage();

            _messageEmitter
                .Messages
                .Should()
                .HaveCount(2);
        }

        [Fact]
        public void GivenBytesForTwoMessages_SecondMessageBytesMatch()
        {
            GivenBytesOnNetwork(new byte[] { 0x2, 0x3, 0x4 }, new byte[] { 0x5, 0x6});

            WhenReceivingMessage();

            _messageEmitter
                .Messages[1]
                .Should()
                .BeEquivalentTo(new byte[] { 0x5, 0x6 });
        }

        [Fact]
        public void Temp()
        {
            var peer0_0 = new byte[] { /* Packet 16 */
                0x00, 0x00, 0x00, 0x18, 0x02, 0x00, 0x00, 0xc7, 
                0x83, 0x31, 0x1f, 0x30, 0xd6, 0xab, 0xdf, 0xaf, 
                0x1e, 0xd9, 0x72, 0x9c, 0x9f, 0x7e, 0xbf, 0xd5, 
                0xaa, 0x15, 0xc0, 0x24 };
            var peer0_1 = new byte[] { /* Packet 18 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0x5d, 0x9f, 0x06, 
                0xbf, 0x48, 0xc6, 0x3d, 0xff, 0x9d, 0x7d, 0xe1, 
                0x5a, 0x9a, 0xa5, 0xa8, 0xd6, 0xe6, 0x2c, 0x89, 
                0xa5, 0x51 };
            var peer0_2 = new byte[] { /* Packet 20 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0x80, 0xe3, 0x35, 
                0x85, 0x77, 0xad, 0xbd, 0x50, 0x33, 0x0f, 0x81, 
                0x29, 0xdc, 0x2b, 0x4c, 0xe8, 0xa9, 0x1d, 0x2a, 
                0x51, 0x51 };
            var peer0_3 = new byte[] { /* Packet 22 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0xa0, 0x54, 0x6b, 
                0x5a, 0xb2, 0x89, 0xab, 0xa9, 0xe7, 0x69, 0x78, 
                0xa9, 0x32, 0x74, 0xea, 0x8c, 0x1f, 0xf5, 0xb1, 
                0xe5, 0xaf };
            var peer0_4 = new byte[] { /* Packet 24 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0x5f, 0x78, 0xe9, 
                0x0c, 0x4c, 0xbc, 0xad, 0xa4, 0xa1, 0x0f, 0x7f, 
                0x47, 0x99, 0x15, 0xfb, 0xd6, 0x69, 0x7c, 0x3f, 
                0x63, 0x89 };
            var peer0_5 = new byte[] { /* Packet 26 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0xdb, 0x1a, 0x14, 
                0x61, 0xc8, 0xf7, 0xfd, 0x4c, 0xb7, 0x17, 0x1f, 
                0x03, 0x8f, 0x04, 0xf6, 0x90, 0x97, 0x95, 0x1b, 
                0x82, 0x71 };
            var peer0_6 = new byte[] { /* Packet 28 */
                0x00, 0x00, 0x00, 0x16, 0x00, 0xfa, 0x19, 0xbb, 
                0xf2, 0xe5, 0x81, 0x6e, 0x98, 0xf8, 0xdf, 0x57, 
                0x2f, 0xfe, 0x56, 0x22, 0x6f, 0x9c, 0xb8, 0x80, 
                0xbc, 0xd7 };

            GivenBytesOnNetwork(peer0_0, peer0_1, peer0_2, peer0_3, peer0_4, peer0_5, peer0_6);

            WhenReceivingMessage();


            foreach (var bytes in _messageEmitter.Messages)
            {
                Debug.WriteLine(Convert.ToBase64String(bytes));
            }
        }

        [Fact]
        public void GivenWrongProtocolVersionInPayload_NoMessageIsEmitted()
        {
            _controllableZwiftCrypto.DecryptionResult = new DecryptionFailedResult("Unsupported protocol version 12");
            
            GivenBytesOnNetwork(new byte[] { 0x2, 0x3, 0x4 }, new byte[] { 0x5, 0x6});

            WhenReceivingMessage();

            _messageEmitter
                .Messages
                .Should()
                .BeEmpty();
        }

        private void WhenReceivingMessage()
        {
            var cancellationTokenSource = new CancellationTokenSource(50);

            try
            {
                _useCase
                    .ExecuteAsync(cancellationTokenSource.Token) // Always in cancelled state because otherwise the use case remains in an infinite loop
                    .GetAwaiter()
                    .GetResult();
            }
            catch (OperationCanceledException)
            {
                // Nop
            }
        }

        private void GivenBytesOnNetwork(params byte[][] bytes)
        {
            var total = bytes
                .Select(CreatePacketPayload)
                .SelectMany(b => b)
                .ToArray();

            _messageReceiver.AvailableBytes = total;
        }

        private static byte[] CreatePacketPayload(byte[] payload)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes((Int16)payload.Length)
                    .Reverse()
                    .Concat(payload)
                    .ToArray();
            }

            return BitConverter.GetBytes((Int16)payload.Length)
                .Concat(payload)
                .ToArray();
        }
    }
}
