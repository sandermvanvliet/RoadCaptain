using System;

namespace RoadCaptain.Adapters
{
    internal class Message
    {
        public string Topic { get; set; }
        public string Data { get; set; }
        public DateTime TimeStamp { get; set; }
        public Type Type { get; set; }
    }
}