using Autofac;
using RoadCaptain.UserInterface.Shared;

namespace RoadCaptain.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext) : base(componentContext)
        {
        }

        public void ShowMainWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                Activate(CurrentWindow);
            }
            else
            {
                var window = Resolve<MainWindow>();

                if (CurrentWindow != null)
                {
                    Close(CurrentWindow);
                }

                Show(window);
            }
        }
    }
}