using Autofac;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.Shared
{
    public class SharedModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateAvailableWindow>().AsSelf();
            builder.RegisterType<WhatIsNewWindow>().AsSelf();
        }
    }
}
