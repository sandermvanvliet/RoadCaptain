using System;

namespace RoadCaptain.Adapters
{
    internal class Message
    {
        public string Topic { get; set; }
        public object Data { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}