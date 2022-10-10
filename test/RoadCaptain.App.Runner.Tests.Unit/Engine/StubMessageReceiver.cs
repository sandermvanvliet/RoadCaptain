// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class StubMessageReceiver : IMessageReceiver
    {
        public byte[]? ReceiveMessageBytes()
        {
            return null;
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Shutdown()
        {
        }

        public event EventHandler? AcceptTimeoutExpired;
        public event EventHandler? DataTimeoutExpired;
        public event EventHandler? ConnectionLost;
        public event EventHandler? ConnectionAccepted;
    }
}
