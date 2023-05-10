namespace RoadCaptain.App.Web.Adapters.EntityFramework
{
    public class User
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? ZwiftSubject { get; set; }
        public string? ZwiftProfileId { get; set; }
        public IEnumerable<Route> Routes { get; set; }
    }
}