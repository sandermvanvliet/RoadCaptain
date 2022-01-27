namespace RoadCaptain
{
    public class ZwiftProfile
    {
        public long Id { get; set; }
        public string PublicId { get; set; }
        public int? WorldId { get; set; }
        public bool Riding { get; set; }
        public bool LikelyInGame { get; set; }
    }
}