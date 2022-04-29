// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using Avalonia.Controls;
using RoadCaptain.App.RouteBuilder.Models;

namespace RoadCaptain.App.RouteBuilder
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
            
            builder.RegisterType<UserPreferences>().AsSelf().SingleInstance();

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.Namespace.EndsWith(".Views") && type.BaseType == typeof(Window))
                .AsSelf();

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.Namespace.EndsWith(".ViewModels"))
                .AsSelf();
        }
    }
}

