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