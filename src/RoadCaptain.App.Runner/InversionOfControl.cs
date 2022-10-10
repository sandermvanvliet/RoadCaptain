// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.Adapters;
using RoadCaptain.App.Shared;
using Serilog;

namespace RoadCaptain.App.Runner
{
    public class InversionOfControl
    {
        public static ContainerBuilder ConfigureContainer(IConfigurationRoot configuration, ILogger logger, Dispatcher dispatcher)
        {
            var builder = new ContainerBuilder();

            builder.Register(_ => logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();

            builder.RegisterType<ZwiftCredentialCache>().As<IZwiftCredentialCache>().SingleInstance();

            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            builder.RegisterModule<DomainModule>();
            builder.RegisterModule<AdaptersModule>();
            builder.RegisterModule<SharedModule>();

            builder.RegisterModule<MainModule>();

            // Register dispatcher here because MainModule does not know of it
            builder.RegisterInstance(dispatcher).AsSelf().SingleInstance();

            return builder;
        }
    }
}
