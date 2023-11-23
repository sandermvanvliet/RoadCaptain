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