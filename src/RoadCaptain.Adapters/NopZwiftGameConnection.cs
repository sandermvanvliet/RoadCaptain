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