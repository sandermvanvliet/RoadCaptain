namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            Route = new RouteViewModel();
        }

        public RouteViewModel Route { get; set; }
    }
}