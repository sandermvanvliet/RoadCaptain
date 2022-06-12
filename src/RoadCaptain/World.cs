namespace RoadCaptain
{
    public class World
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SpawnPoint[] SpawnPoints { get; set; }
        public WorldStatus Status { get; set; }
        public ZwiftWorldId ZwiftId { get; set; } = ZwiftWorldId.Unknown;
    }

    public enum WorldStatus
    {
        Unknown,
        Available,
        Unavailable
    }

    public enum ZwiftWorldId
    {
        Unknown = -1,
        Watopia = 1,
        Richmond = 2,
        London = 3,
        NewYork = 4,
        Innsbruck = 5,
        Bologna = 6,
        Yorkshire = 7,
        CritCity = 8,
        MakuriIslands = 9,
        France = 10,
        Paris = 11
    }
}
