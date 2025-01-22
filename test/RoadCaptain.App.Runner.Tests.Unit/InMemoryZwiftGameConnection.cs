// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit
{
    internal class InMemoryZwiftGameConnection : IZwiftGameConnection
    {
        public void SendInitialPairingMessage(uint riderId, uint sequenceNumber)
        {
        }

        public void SendTurnCommand(TurnDirection direction, ulong sequenceNumber, uint riderId)
        {
            SentCommands.Add(direction.ToString());
        }

        public void EndActivity(ulong sequenceNumber, string activityName, uint riderId)
        {
            SentCommands.Add($"ENDACTIVITY;{activityName}");
        }

        public List<string> SentCommands { get; } = new();
    }
}
