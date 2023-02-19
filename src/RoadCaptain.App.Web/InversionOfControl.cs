using Autofac;
using RoadCaptain.Adapters;

namespace RoadCaptain.App.Web
{
    public class InversionOfControl
    {
        public static void ConfigureContainer(ContainerBuilder builder, Serilog.ILogger logger, IConfiguration configuration)
        {
            builder.Register(_ => logger).SingleInstance();
            builder.Register(_ => configuration).SingleInstance();

            builder.RegisterModule<DomainModule>();
            builder.RegisterModule<AdaptersModule>();
            builder.RegisterModule<MainModule>();
        }
    }
}