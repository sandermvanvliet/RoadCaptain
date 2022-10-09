using Autofac;
using RoadCaptain.App.MacOs.UserPreferences;
using RoadCaptain.App.MacOs.Views;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.MacOs
{
    public class MacOsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MacOsUserPreferences>().As<IUserPreferences>().SingleInstance();
            builder.RegisterType<ZwiftLoginWindow>().As<ZwiftLoginWindowBase>();
        }
    }
}