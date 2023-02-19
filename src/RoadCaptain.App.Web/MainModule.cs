using Autofac;
using RoadCaptain.App.Web.Adapters;
using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MonitoringEventsWithSerilog>()
                .As<MonitoringEvents>()
                .SingleInstance();

            builder
                .RegisterType<SqliteRouteStore>()
                .As<IRouteStore>();

            builder
                .RegisterType<SqliteUserStore>()
                .As<IUserStore>();

            builder
                .RegisterType<RoadCaptainDataContext>()
                .AsSelf()
                .InstancePerLifetimeScope()
                .OnActivated(args => args.Instance.Database.EnsureCreated());
        }
    }
}