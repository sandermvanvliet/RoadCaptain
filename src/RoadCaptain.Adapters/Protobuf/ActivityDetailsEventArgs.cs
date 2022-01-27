using System;

namespace RoadCaptain.Adapters.Protobuf
{
    public class ActivityDetailsEventArgs : EventArgs
    {
        public ulong ActivityId { get; set; }
    }
}