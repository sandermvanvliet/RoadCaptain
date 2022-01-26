using Autofac;

namespace RoadCaptain.Host.Console
{
    internal class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MonitoringEventsWithSerilog>().As<MonitoringEvents>().SingleInstance();
        }
    }
}
