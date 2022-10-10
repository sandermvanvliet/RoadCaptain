// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows.Threading;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace RoadCaptain.RouteBuilder
{
    public class InversionOfControl
    {
        public static ContainerBuilder ConfigureContainer(IConfigurationRoot configuration, ILogger logger, Dispatcher dispatcher)
        {
            var builder = new ContainerBuilder();

            builder.Register(_ => logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();
            
            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            builder.Register(_ =>
            {
                var userPreferences = UserPreferences.Default;

                if (userPreferences.UpgradeSettings)
                {
                    userPreferences.Upgrade();
                    userPreferences.UpgradeSettings = false;
                    userPreferences.Save();
                }

                return userPreferences;
            }).SingleInstance();

            // Register dispatcher here because MainModule does not know of it
            builder.RegisterInstance(dispatcher).AsSelf().SingleInstance();

            return builder;
        }
    }
}
