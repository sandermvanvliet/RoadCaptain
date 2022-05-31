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