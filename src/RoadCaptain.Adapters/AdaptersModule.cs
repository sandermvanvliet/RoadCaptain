using System;
using System.Net.Http;
using Autofac;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public class AdaptersModule : Module
    {
        /// <summary>
        /// Where to read packets from. Can be 'socket' or 'file'
        /// </summary>
        /// <remarks>When set to 'file' <see cref="CaptureFilePath"/> must be set</remarks>
        public string MessageReceiverSource { get; set; } = "socket";
        public string CaptureFilePath { get; set; }

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

            if("socket".Equals(MessageReceiverSource,StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                .RegisterType<MessageReceiverFromSocket>()
                .As<IMessageReceiver>()
                .SingleInstance();
            }
            else if("file".Equals(MessageReceiverSource,StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<MessageReceiverFromCaptureFile>()
                    .As<IMessageReceiver>()
                    .WithParameter("captureFilePath", CaptureFilePath)
                    .SingleInstance();
            }
            else
            {
                throw new InvalidOperationException(
                    $"{nameof(MessageReceiverSource)} must be set to either 'socket' or 'file'");
            }
            
            builder
                .RegisterType<MessageEmitterToQueue>()
                .As<IMessageEmitter>()
                .SingleInstance();
        }
    }
}