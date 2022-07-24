namespace RoadCaptain.Ports
{
    public interface IZwiftCrypto
    {
        byte[] Encrypt(byte[] input);
        byte[] Decrypt(byte[] input);
    }
}