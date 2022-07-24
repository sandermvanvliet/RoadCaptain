namespace RoadCaptain.Commands
{
    public class ConnectCommand
    {
        public string AccessToken { get; set; }
        public byte[]? ConnectionEncryptionSecret { get; set; }
    }
}