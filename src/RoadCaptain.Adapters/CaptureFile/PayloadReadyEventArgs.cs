// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
