using Autofac;
using RoadCaptain.App.Linux.UserPreferences;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Linux
{
    public class LinuxModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LinuxUserPreferences>().As<IUserPreferences>().SingleInstance();
        }
    }
}