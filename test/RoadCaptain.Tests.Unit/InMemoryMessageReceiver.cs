// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageReceiver : IMessageReceiver
    {
        private bool _wasCalled;
        private static readonly object SyncRoot = new();

        public byte[]? ReceiveMessageBytes(string? connectionEncryptionSecret)
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

        public byte[]? AvailableBytes { get; set; }
    }
}
