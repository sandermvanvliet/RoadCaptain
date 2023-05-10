// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.Adapters
{
    internal class Message
    {
        public string? Topic { get; set; }
        public object? Data { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
