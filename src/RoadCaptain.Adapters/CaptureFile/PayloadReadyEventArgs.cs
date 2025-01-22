// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.Adapters.CaptureFile
{
    internal class PayloadReadyEventArgs : EventArgs
    {
        public PayloadReadyEventArgs(uint sequenceNumber, byte[] payload, bool clientToServer)
        {
            SequenceNumber = sequenceNumber;
            Payload = payload;
            ClientToServer = clientToServer;
        }

        public byte[] Payload { get; }
        public uint SequenceNumber { get; }
        public bool ClientToServer { get; }
        public bool ServerToClient => !ClientToServer;
    }
}