namespace RoadCaptain.Ports
{
    /// <summary>
    /// Read raw input from Zwift as it sends it to companion apps
    /// </summary>
    public interface IMessageReceiver
    {
        byte[] ReceiveMessageBytes();
    }
}