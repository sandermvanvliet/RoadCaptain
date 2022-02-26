namespace RoadCaptain.Ports
{
    // TODO: rename this port because we're now also sending on it
    public interface IMessageReceiver
    {
        byte[] ReceiveMessageBytes();
    }
}