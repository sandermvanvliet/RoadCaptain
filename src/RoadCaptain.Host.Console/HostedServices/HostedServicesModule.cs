using Autofac;

namespace RoadCaptain.Host.Console.HostedServices
{
    public class HostedServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Namespace.EndsWith(".HostedServices"))
                .AsImplementedInterfaces();
        }
    }
}