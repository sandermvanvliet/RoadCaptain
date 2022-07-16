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
        /// <param name="connectionEncryptionSecret"></param>
        /// <returns>An array of bytes with a payload or <c>null</c> if the connection is closed</returns>
        /// <remarks>This method blocks while waiting for new input</remarks>
        byte[]? ReceiveMessageBytes(string? connectionEncryptionSecret);

        /// <summary>
        /// Stop the message receiver from accepting data from a Zwift connection
        /// </summary>
        void Shutdown();
    }
}