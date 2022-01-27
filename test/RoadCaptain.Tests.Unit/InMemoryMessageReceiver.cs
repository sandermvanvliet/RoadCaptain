using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageReceiver : IMessageReceiver
    {
        private bool _wasCalled;
        private static readonly object SyncRoot = new();

        public byte[] ReceiveMessageBytes()
        {
            // Note: This method should only return the bytes once.
            //       The next call should pretend that all bytes have
            //       been read and there is no more data.
            if (_wasCalled)
            {
                return null;
            }

            lock (SyncRoot)
            {
                _wasCalled = true;
            }

            return AvailableBytes;
        }

        public void Shutdown()
        {
        }

        public void SendMessageBytes(byte[] payload)
        {
        }

        public void SendInitialPairingMessage(uint riderId)
        {
        }

        public byte[] AvailableBytes { get; set; }
    }
}