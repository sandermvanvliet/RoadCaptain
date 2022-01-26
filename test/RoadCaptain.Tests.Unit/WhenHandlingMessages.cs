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
        public void GivenBytesAvailable_MessageIsEmitted()
        {
            GivenBytesOnNetwork();

            WhenReceivingMessage();

            _messageEmitter
                .Messages
                .Should()
                .HaveCount(1);
        }

        private void WhenReceivingMessage()
        {
            _useCase.Execute(new CancellationToken(true)); // Always in cancelled state because otherwise the usecasse remains in an infinite loop
        }

        private void GivenBytesOnNetwork()
        {
            _messageReceiver.AvailableBytes = new byte[] { 0x1, 0x2, 0x3 };
        }
    }
}