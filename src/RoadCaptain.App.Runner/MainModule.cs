// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using Autofac.Core.Activators.Reflection;
using Avalonia.Controls;
using RoadCaptain.App.Runner.ViewModels;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Module = Autofac.Module;

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

            string? platformAssemblyPath = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformAssemblyPath = Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.Windows.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformAssemblyPath = Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.Linux.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformAssemblyPath = Path.Combine(Environment.CurrentDirectory, "RoadCaptain.App.MacOs.dll");
            }

            if (!string.IsNullOrEmpty(platformAssemblyPath))
            {
                // This is inside a File.Exists check to allow this part
                // to run under unit tests when working in the generic solution
                if (File.Exists(platformAssemblyPath))
                {
                    var platformAssembly = Assembly.LoadFile(platformAssemblyPath);
                    builder.RegisterAssemblyModules(platformAssembly);
                }
            }
            else
            {
                throw new Exception("Unable to determine platform, can't load platform specific application components and I refuse to start");
            }

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(Window) && (type.Namespace ?? "").EndsWith(".Views"))
                .UsingConstructor(new MostParametersConstructorSelector())
                .AsSelf();
            
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.BaseType == typeof(ViewModelBase) && (type.Namespace ?? "").EndsWith(".ViewModels"))
                .AsSelf();

            builder.RegisterType<Engine>().AsSelf().SingleInstance();
        }
    }
}

