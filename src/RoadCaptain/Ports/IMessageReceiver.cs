namespace RoadCaptain.Ports
{
    public interface IMessageReceiver
    {
        byte[] ReceiveMessageBytes();
        void Shutdown();

        void SendMessageBytes(byte[] payload);
        void SendInitialPairingMessage(uint riderId);
    }
}