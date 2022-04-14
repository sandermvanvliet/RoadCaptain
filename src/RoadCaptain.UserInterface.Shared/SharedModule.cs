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
