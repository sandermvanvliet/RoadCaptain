// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Runner
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

#if WIN
            builder.RegisterModule(new RoadCaptain.App.Windows.WindowsModule());
#elif LINUX
            builder.RegisterModule(new RoadCaptain.App.Linux.LinuxModule());
#elif MACOS
            builder.RegisterModule(new RoadCaptain.App.MacOs.MacOsModule());
#else
            builder.RegisterType<DummyUserPreferences>().As<IUserPreferences>().SingleInstance();
#endif
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type != typeof(Configuration) &&
                               type != typeof(MonitoringEventsWithSerilog) &&
                               type != typeof(WindowService))
                .AsSelf();
        }
    }
}

