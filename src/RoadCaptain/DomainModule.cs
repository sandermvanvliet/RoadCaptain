using Autofac;

namespace RoadCaptain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Namespace != null && t.Namespace.EndsWith(".UseCases"))
                .AsSelf();
        }
    }
}