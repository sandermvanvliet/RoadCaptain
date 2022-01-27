using System.Net.Http;
using Autofac;
using RoadCaptain.Ports;

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
                .AsImplementedInterfaces()
                .Except<MessageReceiverFromSocket>()
                .Except<MessageEmitterToQueue>();

            builder
                .RegisterType<MessageReceiverFromSocket>()
                .As<IMessageReceiver>()
                .SingleInstance();
            
            builder
                .RegisterType<MessageEmitterToQueue>()
                .As<IMessageEmitter>()
                .SingleInstance();
        }
    }
}