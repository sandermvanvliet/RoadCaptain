namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeOpenRouteDialogViewModel : OpenRouteDialogViewModel
    {
        public DesignTimeOpenRouteDialogViewModel() : base(new DesignTimeWindowService(), new DummyUserPreferences(), null)
        {
        }
    }
}