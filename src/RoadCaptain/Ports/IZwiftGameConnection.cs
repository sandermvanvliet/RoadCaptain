namespace RoadCaptain.Ports
{
    public interface IZwiftGameConnection
    {
        void SendInitialPairingMessage(uint riderId, uint sequenceNumber);
        void SendTurnCommand(TurnDirection direction, ulong sequenceNumber, uint riderId);
        void EndActivity(ulong sequenceNumber, string activityName, uint riderId);
    }
}