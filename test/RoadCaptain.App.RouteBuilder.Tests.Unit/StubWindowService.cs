using Autofac;
using JetBrains.Annotations;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class StubWindowService : WindowService
    {
        public StubWindowService([NotNull] IComponentContext componentContext,
            [NotNull] MonitoringEvents monitoringEvents) : base(componentContext, monitoringEvents)
        {
        }
    }
}