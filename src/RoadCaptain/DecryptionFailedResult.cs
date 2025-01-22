// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using RoadCaptain.Ports;

namespace RoadCaptain
{
    public class DecryptionFailedResult : DecryptionResult
    {
        public DecryptionFailedResult(string reason)
        {
            Reason = reason;
        }

        public DecryptionFailedResult(string reason, Exception exception)
        {
            Reason = reason;
            Exception = exception;
        }

        public override bool IsSuccess => false;
        public string Reason { get; }
        public Exception? Exception { get; }
    }
}
