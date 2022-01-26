using Autofac;
using RoadCaptain.UseCases;

namespace RoadCaptain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HandleIncomingMessageUseCase>().AsSelf();
        }
    }
}