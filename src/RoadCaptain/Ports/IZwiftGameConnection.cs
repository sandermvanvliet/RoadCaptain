// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Ports
{
    public interface IZwiftGameConnection
    {
        void SendInitialPairingMessage(uint riderId, uint sequenceNumber);
        void SendTurnCommand(TurnDirection direction, ulong sequenceNumber, uint riderId);
        void EndActivity(ulong sequenceNumber, string activityName, uint riderId);
    }
}
