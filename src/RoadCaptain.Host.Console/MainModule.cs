// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Autofac;
using Microsoft.Extensions.Hosting;
using RoadCaptain.Host.Console.HostedServices;

namespace RoadCaptain.Host.Console
{
    internal class MainModule : Module
    {
        /// <summary>
        /// Determine whether to use 'console' or 'winforms' mode for the UI
        /// </summary>
        /// <remarks>When using 'winforms' the console is still shown for log output</remarks>
        public string UserInterfaceMode { get; set; } = "console";

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
            var registrationBuilder = builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Namespace.EndsWith(".HostedServices"));
            
            if ("winforms".Equals(UserInterfaceMode, StringComparison.InvariantCultureIgnoreCase))
            {
                // ... but only register form when we want that mode...
                builder
                    .RegisterType<MainWindow>()
                    .AsSelf();
            }
            else
            {
                // ... and exclude the UI service when we're using console mode...
                registrationBuilder = registrationBuilder.Except<UserInterfaceService>();

                // Ensure that services start immediately
                synchronizer.TriggerSynchronizationEvent();
            }
            
            // ... and finally build up the hosted services
            registrationBuilder.As<IHostedService>();
        }
    }
}

