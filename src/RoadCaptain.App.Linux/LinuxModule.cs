// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.App.Linux.UserPreferences;
using RoadCaptain.App.Shared;

namespace RoadCaptain.App.Linux
{
    public class LinuxModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LinuxUserPreferences>().As<IUserPreferences>().SingleInstance();
            builder.RegisterType<InMemoryZwiftCredentialCache>().As<IZwiftCredentialCache>().SingleInstance();
        }
    }
}
