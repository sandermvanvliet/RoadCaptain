using System;

namespace RoadCaptain.Adapters.Protobuf
{
    public class CommandSentEventArgs : EventArgs
    {
        public CommandType CommandType { get; set; }
    }
}