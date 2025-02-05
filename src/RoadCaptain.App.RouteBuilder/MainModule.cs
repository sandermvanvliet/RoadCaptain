// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Avalonia.Controls;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.App.Shared.ViewModels;
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
            builder.RegisterType<WindowService>()
                .As<IWindowService>()
                .As<Shared.IWindowService>()
                .SingleInstance();
            
            builder.RegisterType<ViewLocator>().AsSelf().SingleInstance();

            builder.RegisterType<StatusBarService>().As<IStatusBarService>().SingleInstance();
            builder.RegisterDecorator<DelegateDecorator, IWindowService>();

            RegisterViews(builder);

            string? platformAssemblyPath = null;
            var thisAssemblyLocation = Path.GetDirectoryName(GetType().Assembly.Location);

            if (string.IsNullOrEmpty(thisAssemblyLocation))
            {
                throw new Exception(
                    "Unable to determine the location of the RoadCaptain Route Builder assembly which means I can't initialize properly");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformAssemblyPath = Path.Combine(thisAssemblyLocation, "RoadCaptain.App.Windows.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformAssemblyPath = Path.Combine(thisAssemblyLocation, "RoadCaptain.App.Linux.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformAssemblyPath = Path.Combine(thisAssemblyLocation, "RoadCaptain.App.MacOs.dll");
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
                throw new Exception(
                    "Unable to determine platform, can't load platform specific application components and I refuse to start");
            }

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(type =>
                    type.BaseType == typeof(ViewModelBase) && type.Namespace != null &&
                    type.Namespace.EndsWith(".ViewModels"))
                .AsSelf();
        }

        private void RegisterViews(ContainerBuilder builder)
        {
            var viewTypes = ThisAssembly
                .GetTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false, Namespace: not null } &&
                    (type.BaseType == typeof(Window) || type.BaseType == typeof(UserControl)) &&
                    type.Namespace.EndsWith(".Views"))
                .ToList();

            foreach (var viewType in viewTypes)
            {
                builder
                    .RegisterType(viewType)
                    .UsingConstructor(new MostParametersConstructorSelector())
                    .AsSelf();
            }
        }
    }
}