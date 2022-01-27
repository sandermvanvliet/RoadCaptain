using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenHandlingMessages
    {
        private readonly InMemoryMessageReceiver _messageReceiver;
        private readonly HandleIncomingMessageUseCase _useCase;
        private readonly InMemoryMessageEmitter _messageEmitter;

        public WhenHandlingMessages()
        {
            _messageReceiver = new InMemoryMessageReceiver();
            _messageEmitter = new InMemoryMessageEmitter();
            _useCase = new HandleIncomingMessageUseCase(_messageReceiver, _messageEmitter);
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
            Thread.Sleep(50);
            _messageEmitter
                .Messages
                .Single()
                .Should()
                .BeEquivalentTo(new byte[] {0x2, 0x3, 0x4 });
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

        private void WhenReceivingMessage()
        {
            var cancellationTokenSource = new CancellationTokenSource(50);

            try
            {
                _useCase
                    .ExecuteAsync(cancellationTokenSource.Token) // Always in cancelled state because otherwise the usecasse remains in an infinite loop
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