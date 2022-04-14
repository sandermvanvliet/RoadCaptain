namespace RoadCaptain.Runner
{
    internal class ReleaseResponse
    {
        public string Id { get; set; }
        public string TagName { get; set; }
        public string Body { get; set; }
        public ReleaseAsset[] Assets { get; set; }
    }
}