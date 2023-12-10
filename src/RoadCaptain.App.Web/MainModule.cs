// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using Microsoft.EntityFrameworkCore;
using RoadCaptain.App.Web.Adapters;
using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MonitoringEventsWithSerilog>()
                .As<MonitoringEvents>()
                .SingleInstance();

            builder
                .RegisterType<SqliteRouteStore>()
                .As<IRouteStore>();

            builder
                .RegisterType<SqliteUserStore>()
                .As<IUserStore>();

            builder
                .RegisterType<RoadCaptainDataContext>()
                .AsSelf()
                .InstancePerLifetimeScope()
                .OnActivated(args =>
                {
                    // If the database already exists we need to add the initial migration
                    // to it and pretend it has already run (which it did but I forgot to 
                    // add migrations...)
                    try
                    {
                        args.Instance.Database.ExecuteSqlRaw(
                            @"INSERT INTO __EFMigrationsHistory
SELECT '20231210130249_InitialSchema', '7.0.3'
WHERE NOT EXISTS(SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20231210130249_InitialSchema')");
                    }
                    catch
                    {
                        //Nop
                    }

                    args.Instance.Database.Migrate();
                });
        }
    }
}
