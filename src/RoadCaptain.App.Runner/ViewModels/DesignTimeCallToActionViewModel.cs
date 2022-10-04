using Avalonia.Media;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeCallToActionViewModel : CallToActionViewModel
    {
        public DesignTimeCallToActionViewModel()
            : base(
                "Waiting for connection", 
                "Start Zwift and start cycling in Watopia on route The Mega Pretzel")
        {
        }
    }
}