using Autofac;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.App.Windows.UserPreferences;

namespace RoadCaptain.App.Windows
{
    public class WindowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsUserPreferences>().As<IUserPreferences>().SingleInstance();
        }
    }
}