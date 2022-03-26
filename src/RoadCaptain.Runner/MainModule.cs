// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
            
            builder.RegisterType<WindowService>().As<IWindowService>();
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type != typeof(Configuration) &&
                               type != typeof(MonitoringEventsWithSerilog) &&
                               type != typeof(WindowService) &&
                               !type.Namespace.EndsWith(".HostedServices"))
                .AsSelf();
        }
    }
}

