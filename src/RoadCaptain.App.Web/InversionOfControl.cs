// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.Adapters;

namespace RoadCaptain.App.Web
{
    public class InversionOfControl
    {
        public static void ConfigureContainer(ContainerBuilder builder, Serilog.ILogger logger, IConfiguration configuration)
        {
            builder.Register(_ => logger).SingleInstance();
            builder.Register(_ => configuration).SingleInstance();

            builder.RegisterModule<DomainModule>();
            builder.RegisterModule(new AdaptersModule(configuration));
            builder.RegisterModule<MainModule>();
        }
    }
}
