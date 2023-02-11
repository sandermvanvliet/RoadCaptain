// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Avalonia.Controls;
using RoadCaptain.App.RouteBuilder.ViewModels;
using Module = Autofac.Module;

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

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(Window) && type.Namespace != null && type.Namespace.EndsWith(".Views"))
                .UsingConstructor(new MostParametersConstructorSelector())
                .AsSelf();

            Assembly platformAssembly;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformAssembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.Windows.dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformAssembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.Linux.dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformAssembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.MacOs.dll"));
            }
            else
            {
                throw new Exception("Unable to determine the platform assembly to load!");
            }

            builder.RegisterAssemblyModules(platformAssembly);
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(ViewModelBase) && type.Namespace != null && type.Namespace.EndsWith(".ViewModels"))
                .AsSelf();
        }
    }
}

