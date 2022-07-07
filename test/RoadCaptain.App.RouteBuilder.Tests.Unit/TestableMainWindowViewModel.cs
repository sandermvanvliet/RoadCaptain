using System.Diagnostics;
using System.Threading.Tasks;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class TestableMainWindowViewModel : MainWindowViewModel
    {
        public TestableMainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker,
            IWindowService windowService, IWorldStore worldStore, IUserPreferences userPreferences)
            : base(routeStore, segmentStore, versionChecker, windowService, worldStore, userPreferences)
        {
        }

        [DebuggerStepThrough]
        public async Task<CommandResult> CallAddSegmentToRoute(Segment segment)
        {
            return await SelectSegment(segment);
        }
    }
}