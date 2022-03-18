namespace RoadCaptain
{
    public abstract class ZwiftMessage
    {
    }

    public sealed class ZwiftRiderPositionMessage : ZwiftMessage
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
    }

    public sealed class ZwiftCommandAvailableMessage : ZwiftMessage
    {
        public string Type { get; set; }
        public ulong SequenceNumber { get; set; }
    }

    public sealed class ZwiftPowerUpMessage : ZwiftMessage
    {
        public string Type { get; set; }
    }

    public sealed class ZwiftPingMessage : ZwiftMessage
    {
        public uint RiderId { get; set; }
    }

    public sealed class ZwiftActivityDetailsMessage : ZwiftMessage
    {
        public ulong ActivityId { get; set; }
        public uint RiderId { get; set; }
    }
}