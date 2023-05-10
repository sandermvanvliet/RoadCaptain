namespace RoadCaptain.App.Web.Models
{
    public class CreateRouteModel
    {
        public string? Name { get; set; }
        public string? ZwiftRouteName { get; set; }
        public decimal Distance { get; set; }
        public decimal Ascent { get; set; }
        public decimal Descent { get; set; }
        public bool IsLoop { get; set; }
        public string? Serialized { get; set; }
    }
}