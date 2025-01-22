// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class StubMessageReceiver : IMessageReceiver
    {
        public byte[]? ReceiveMessageBytes()
        {
            return null;
        }

        public void Shutdown()
        {
        }
    }
}
