using System;

namespace RoadCaptain.Adapters.Protobuf
{
    public class CommandAvailableEventArgs : EventArgs
    {
        public CommandType CommandType { get; set; }
    }
}