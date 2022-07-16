﻿using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class StubMessageReceiver : IMessageReceiver
    {
        public byte[]? ReceiveMessageBytes(string? connectionEncryptionSecret)
        {
            return null;
        }

        public void Shutdown()
        {
        }
    }
}