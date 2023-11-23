namespace RoadCaptain
{
    public class SuccessfulDecryptionResult : DecryptionResult
    {
        public SuccessfulDecryptionResult(byte[] data)
        {
            Data = data;
        }

        public override bool IsSuccess => true;
        public byte[] Data { get; }
    }
}