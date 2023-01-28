// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows;
using Autofac;

namespace RoadCaptain.UserInterface.Shared
{
    internal class SharedModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register windows
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.BaseType == typeof(Window))
                .AsSelf();
        }
    }
}

