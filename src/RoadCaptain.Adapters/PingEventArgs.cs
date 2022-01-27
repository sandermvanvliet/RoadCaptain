using System;

namespace RoadCaptain.Adapters
{
    internal class PingEventArgs : EventArgs
    {
        public uint RiderId { get; set; }
    }
}