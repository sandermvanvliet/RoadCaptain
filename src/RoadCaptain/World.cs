namespace RoadCaptain
{
    public class World
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SpawnPoint[] SpawnPoints { get; set; }

        public string Image
        {
            get
            {
                return $"pack://application:,,,/RoadCaptain.UserInterface.Shared;component/Assets/world-{Id}.jpg";
            }
        }
    }
}
