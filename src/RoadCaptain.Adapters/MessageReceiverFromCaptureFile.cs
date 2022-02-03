using System;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageReceiverFromCaptureFile : IMessageReceiver
    {
        private readonly string _captureFilePath;

        public MessageReceiverFromCaptureFile(string captureFilePath)
        {
            _captureFilePath = captureFilePath;
        }

        public byte[] ReceiveMessageBytes()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void SendMessageBytes(byte[] payload)
        {
            throw new NotImplementedException();
        }

        public void SendInitialPairingMessage(uint riderId)
        {
            throw new NotImplementedException();
        }
    }
}
