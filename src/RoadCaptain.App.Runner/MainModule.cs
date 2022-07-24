// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using Autofac.Core.Activators.Reflection;
using Avalonia.Controls;
using RoadCaptain.App.Runner.ViewModels;

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

            // Note: If you think "why not RuntimeInformation.IsOSPlatform()?" then
            //       realize that the platform specific projects are only referenced
            //       for a particular platform _at build time_ and not at runtime.
            //       So if you would want to use a runtime check here that means
            //       that for example the macOS project would need to be included
            //       in the build for Windows and that defeats the purpose of all this.
#if WIN
            builder.RegisterModule(new RoadCaptain.App.Windows.WindowsModule());
#elif LINUX
            builder.RegisterModule(new RoadCaptain.App.Linux.LinuxModule());
#elif MACOS
            builder.RegisterModule(new RoadCaptain.App.MacOs.MacOsModule());
#else
            builder.RegisterType<DummyUserPreferences>().As<RoadCaptain.IUserPreferences>().SingleInstance();
#endif
            
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(Window) && type.Namespace.EndsWith(".Views"))
                .UsingConstructor(new MostParametersConstructorSelector())
                .AsSelf();
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(ViewModelBase) && type.Namespace.EndsWith(".ViewModels"))
                .AsSelf();

            builder.RegisterType<Engine>().AsSelf().SingleInstance();
        }
    }
}

