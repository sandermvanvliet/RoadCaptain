// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Autofac;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.ViewModels;
using RoadCaptain.App.Shared.Views;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Shared
{
    public class SharedModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateAvailableWindow>().AsSelf();
            builder.RegisterType<WhatIsNewWindow>().AsSelf();
            builder.RegisterType<CompileTimeFeatures>().As<IApplicationFeatures>();
            builder.RegisterType<PlatformPaths>().As<IPathProvider>();
            builder.RegisterType<SelectRouteWindow>().AsSelf();
            builder.RegisterType<SelectRouteWindowViewModel>().AsSelf();
            
            // This requires that the depending apps register
            // the shared IWindowService too...
            builder.RegisterType<AppSecurityTokenProvider>().As<ISecurityTokenProvider>();
        }
    }
}

