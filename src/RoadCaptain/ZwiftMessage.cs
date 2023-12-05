// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain
{
    public abstract class ZwiftMessage
    {
    }

    public sealed class ZwiftRiderPositionMessage : ZwiftMessage
    {
        public float X { get; init; }
        public float Y { get; init; }
        public float Z { get; init; }
    }

    public sealed class ZwiftCommandAvailableMessage : ZwiftMessage
    {
        public ZwiftCommandAvailableMessage(string type, ulong sequenceNumber)
        {
            Type = type;
            SequenceNumber = sequenceNumber;
        }

        public string Type { get; }
        public ulong SequenceNumber { get; }
    }

    public sealed class ZwiftPowerUpMessage : ZwiftMessage
    {
        public string? Type { get; init; }
    }

    public sealed class ZwiftPingMessage : ZwiftMessage
    {
        public uint RiderId { get; init; }
    }

    public sealed class ZwiftActivityDetailsMessage : ZwiftMessage
    {
        public ulong ActivityId { get; init; }
        public uint RiderId { get; init; }
    }
}
