using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryZwiftGameConnection : IZwiftGameConnection
    {
        public void SendInitialPairingMessage(uint riderId, uint sequenceNumber)
        {
        }

        public void SendTurnCommand(TurnDirection direction, ulong sequenceNumber)
        {
            SentCommands.Add(direction.ToString());
        }

        public List<string> SentCommands { get; } = new();
    }
}