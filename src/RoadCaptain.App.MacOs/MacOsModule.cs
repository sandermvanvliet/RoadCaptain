// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.App.MacOs.UserPreferences;
using RoadCaptain.App.MacOs.Views;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.MacOs
{
    public class MacOsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MacOsUserPreferences>().As<IUserPreferences>().SingleInstance();
            builder.RegisterType<ZwiftLoginWindow>().As<ZwiftLoginWindowBase>();
            builder.RegisterType<InMemoryZwiftCredentialCache>().As<IZwiftCredentialCache>().SingleInstance();
        }
    }
}
