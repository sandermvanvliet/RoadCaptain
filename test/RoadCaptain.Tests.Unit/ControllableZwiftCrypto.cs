// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    public class ControllableZwiftCrypto : IZwiftCrypto
    {
        public DecryptionResult? DecryptionResult { get; set; }
        public byte[]? EncryptionResult { get; set; }

        public byte[] Encrypt(byte[] input)
        {
            return EncryptionResult ?? input;
        }

        public DecryptionResult Decrypt(byte[] input)
        {
            return DecryptionResult ?? new SuccessfulDecryptionResult(input);
        }

    }
}
