using Autofac;
using RoadCaptain.App.MacOs.UserPreferences;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.MacOs
{
    public class MacOsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MacOsUserPreferences>().As<IUserPreferences>().SingleInstance();
        }
    }
}