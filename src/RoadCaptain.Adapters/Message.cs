// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.Adapters
{
    internal class Message
    {
        public Message(string topic, DateTime timeStamp, object data)
        {
            Topic = topic;
            TimeStamp = timeStamp;
            Data = data;
        }

        public string Topic { get;  }
        public object Data { get; }
        public DateTime TimeStamp { get; }
    }
}
