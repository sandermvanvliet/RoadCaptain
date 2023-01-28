// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows.Threading;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace RoadCaptain.Runner
{
    public class InversionOfControl
    {
        public static ContainerBuilder ConfigureContainer(IConfigurationRoot configuration, ILogger logger, Dispatcher dispatcher)
        {
            var builder = new ContainerBuilder();

            builder.Register(_ => logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();

            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            builder.Register(_ =>
            {
                var appSettings = AppSettings.Default;
                
                if(appSettings.UpgradeSettings)
                {
                    appSettings.Upgrade();
                    appSettings.UpgradeSettings = false;
                    appSettings.Save();
                }

                return appSettings;
            }).SingleInstance();
            
            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            // Register dispatcher here because MainModule does not know of it
            builder.RegisterInstance(dispatcher).AsSelf().SingleInstance();

            return builder;
        }
    }
}
