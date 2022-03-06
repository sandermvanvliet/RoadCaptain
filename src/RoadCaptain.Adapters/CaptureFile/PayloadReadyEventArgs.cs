using System;

namespace RoadCaptain.Adapters.CaptureFile
{
    internal class PayloadReadyEventArgs : EventArgs {
        public byte[] Payload { get; set; }
        public uint SequenceNumber { get; set; }
        public bool ClientToServer { get; set; }
        public bool ServerToClient => !ClientToServer;
    }
}