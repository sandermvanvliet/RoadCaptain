namespace RoadCaptain.App.Web.Adapters.EntityFramework
{
    public class Route
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string ZwiftRouteName { get; set; }
        public decimal Distance { get; set; }
        public decimal Ascent { get; set; }
        public decimal Descent { get; set; }
        public bool IsLoop { get; set; }
        public string Serialized { get; set; }
    }
}