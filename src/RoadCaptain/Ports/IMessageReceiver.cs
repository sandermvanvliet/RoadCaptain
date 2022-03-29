namespace RoadCaptain.Ports
{
    /// <summary>
    /// Read raw input from Zwift as it sends it to companion apps
    /// </summary>
    public interface IMessageReceiver
    {
        /// <summary>
        /// Read raw payload data from a Zwift connection
        /// </summary>
        /// <returns>An array of bytes with a payload or <c>null</c> if the connection is closed</returns>
        /// <remarks>This method blocks while waiting for new input</remarks>
        byte[] ReceiveMessageBytes();

        /// <summary>
        /// Stop the message receiver from accepting data from a Zwift connection
        /// </summary>
        void Shutdown();
    }
}