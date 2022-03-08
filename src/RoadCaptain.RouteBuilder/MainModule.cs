using Autofac;

namespace RoadCaptain.RouteBuilder
{
    internal class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MonitoringEventsWithSerilog>()
                .As<MonitoringEvents>()
                .SingleInstance();

            // Register the hosted services...
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AsSelf();
        }
    }
}
