using System;
using Autofac;
using RoadCaptain.Host.Console.HostedServices;
using RoadCaptain.Monitor;

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
            }
            
            // ... and finally build up the hosted services
            registrationBuilder.AsImplementedInterfaces();
        }
    }
}
