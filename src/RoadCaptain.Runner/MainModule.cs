// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;

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
            
            builder.RegisterType<WindowService>().As<IWindowService>();
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type != typeof(Configuration) &&
                               type != typeof(MonitoringEventsWithSerilog) &&
                               type != typeof(WindowService))
                .AsSelf();
        }
    }
}

