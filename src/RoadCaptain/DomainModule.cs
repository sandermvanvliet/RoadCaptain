using Autofac;

namespace RoadCaptain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(this.GetType().Assembly)
                .Where(t => t.Namespace.EndsWith("UseCases"))
                .AsSelf();
        }
    }
}