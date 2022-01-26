namespace RoadCaptain.Ports
{
    public interface IMessageReceiver
    {
        byte[] ReceiveMessageBytes();
    }
}