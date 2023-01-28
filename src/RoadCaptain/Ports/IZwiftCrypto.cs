// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.Ports
{
    public interface IZwiftCrypto
    {
        byte[] Encrypt(byte[] input);
        DecryptionResult Decrypt(byte[] input);
    }

    public abstract class DecryptionResult
    {
        public abstract bool IsSuccess { get; }
    }

    public class SuccessfulDecryptionResult : DecryptionResult
    {
        public SuccessfulDecryptionResult(byte[] data)
        {
            Data = data;
        }

        public override bool IsSuccess => true;
        public byte[] Data { get; }
    }

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
