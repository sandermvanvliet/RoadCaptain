using System.Net.Http;
using Autofac;

namespace RoadCaptain.Adapters
{
    public class AdaptersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(_ => new HttpClient())
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces();
        }
    }
}