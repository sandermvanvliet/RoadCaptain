// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class NopZwiftGameConnection : IZwiftGameConnection
    {
        public void SendInitialPairingMessage(uint riderId, uint sequenceNumber)
        {
        }

        public void SendTurnCommand(TurnDirection direction, ulong sequenceNumber, uint riderId)
        {
        }
    }
}
