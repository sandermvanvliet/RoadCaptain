// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        byte[]? ReceiveMessageBytes();

        /// <summary>
        /// Stop the message receiver from accepting data from a Zwift connection
        /// </summary>
        void Shutdown();
    }
}
