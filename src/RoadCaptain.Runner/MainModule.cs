using Autofac;
using Microsoft.Extensions.Hosting;
using RoadCaptain.Runner.HostedServices;

namespace RoadCaptain.Runner
{
    internal class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MonitoringEventsWithSerilog>()
                .As<MonitoringEvents>()
                .SingleInstance();

            // There should only ever be one synchronizer
            var synchronizer = new Synchronizer();
            builder
                .Register(_ => synchronizer)
                .As<ISynchronizer>()
                .SingleInstance();

            // Register the hosted services...
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Namespace.EndsWith(".HostedServices"))
                .As<IHostedService>();

            // Register everything except the hosted services
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(t => !t.Namespace.EndsWith(".HostedServices") && t.Name != nameof(Configuration))
                .AsSelf();
        }
    }
}
