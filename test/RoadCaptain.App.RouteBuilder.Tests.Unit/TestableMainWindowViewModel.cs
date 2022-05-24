using System.Diagnostics;
using RoadCaptain.Ports;
using RoadCaptain.RouteBuilder.ViewModels;
using RoadCaptain.UserInterface.Shared.Commands;

namespace RoadCaptain.RouteBuilder.Tests.Unit
{
    public class TestableMainWindowViewModel : MainWindowViewModel
    {
        public TestableMainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker,
            IWindowService windowService, IWorldStore worldStore, UserPreferences userPreferences)
            : base(routeStore, segmentStore, versionChecker, windowService, worldStore, userPreferences)
        {
        }

        [DebuggerStepThrough]
        public CommandResult CallAddSegmentToRoute(Segment segment)
        {
            return AddSegmentToRoute(segment);
        }
    }
}