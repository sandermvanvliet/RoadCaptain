using Autofac;

namespace RoadCaptain.Adapters
{
    public class AdaptersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces();
        }
    }
}