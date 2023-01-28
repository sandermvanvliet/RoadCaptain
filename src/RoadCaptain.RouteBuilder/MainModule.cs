// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.UserInterface.Shared;

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
            
            // Single instance because we keep track of the active window
            builder.RegisterType<WindowService>().As<IWindowService>().SingleInstance();
            builder.RegisterDecorator<DelegateDecorator, IWindowService>();

            // Register the hosted services...
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type != typeof(MonitoringEventsWithSerilog) &&
                               type != typeof(WindowService))
                .AsSelf();
        }
    }
}

