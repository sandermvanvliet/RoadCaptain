using System;

namespace RoadCaptain.Adapters
{
    internal class PayloadReadyEventArgs : EventArgs {
        public byte[] Payload { get; set; }
        public uint SequenceNumber { get; set; }
    }
}