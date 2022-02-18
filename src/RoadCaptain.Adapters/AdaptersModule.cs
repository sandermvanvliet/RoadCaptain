using System;
using System.Net;
using System.Net.Http;
using Autofac;
using Codenizer.HttpClient.Testable;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public class AdaptersModule : Module
    {
        /// <summary>
        ///     Where to read packets from. Can be 'socket' or 'file'
        /// </summary>
        /// <remarks>When set to 'file' <see cref="CaptureFilePath" /> must be set</remarks>
        public string MessageReceiverSource { get; set; } = "socket";

        /// <summary>
        /// The path to the NPCAP file containing the TCP packets to replay
        /// </summary>
        public string CaptureFilePath { get; set; }

        public string GameStateBacking { get; set; } = "in-memory";

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces()
                .Except<MessageReceiverFromSocket>()
                .Except<MessageEmitterToQueue>()
                .Except<IGameStateDispatcher>()
                .Except<IGameStateReceiver>();

            if ("socket".Equals(MessageReceiverSource, StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<MessageReceiverFromSocket>()
                    .As<IMessageReceiver>()
                    .SingleInstance();
                
                builder
                    .Register(_ => new HttpClient())
                    .InstancePerLifetimeScope();
            }
            else if ("file".Equals(MessageReceiverSource, StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<MessageReceiverFromCaptureFile>()
                    .As<IMessageReceiver>()
                    .WithParameter("captureFilePath", CaptureFilePath)
                    .SingleInstance();

                var testableHandler = TestableHandlerForZwiftApiCalls();

                builder
                    .Register(_ => new HttpClient(testableHandler))
                    .InstancePerLifetimeScope();
            }
            else
            {
                throw new InvalidOperationException(
                    $"{nameof(MessageReceiverSource)} must be set to either 'socket' or 'file'");
            }

            if ("in-memory".Equals(GameStateBacking, StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<InMemoryGameStateDispatcher>()
                    .As<IGameStateDispatcher>()
                    .As<IGameStateReceiver>()
                    .SingleInstance();
            }
            else if ("zeromq".Equals(GameStateBacking, StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<ZeroMqGameStateDispatcher>()
                    .As<IGameStateDispatcher>()
                    .SingleInstance();
                
                builder
                    .RegisterType<ZeroMqGameStateReceiver>()
                    .As<IGameStateReceiver>()
                    .SingleInstance();
            }
            else
            {
                throw new InvalidOperationException(
                    $"{nameof(GameStateBacking)} must be set to either 'in-memory' or 'zeromq'");
            }

            builder
                .RegisterType<MessageEmitterToQueue>()
                .As<IMessageEmitter>()
                .SingleInstance();
        }

        private static TestableMessageHandler TestableHandlerForZwiftApiCalls()
        {
            var handler = new TestableMessageHandler();

            // Access token
            handler
                .RespondTo()
                .Post()
                .ForUrl("/auth/realms/zwift/protocol/openid-connect/token")
                .With(HttpStatusCode.OK)
                .AndJsonContent(new TokenResponse
                {
                    AccessToken = "at",
                    ExpiresIn = 1,
                    RefreshExpiresIn = 1,
                    RefreshToken = "rt"
                });

            // Zwift servers
            handler
                .RespondTo()
                .Get()
                .ForUrl("/api/servers")
                .With(HttpStatusCode.OK)
                .AndJsonContent(new { baseUrl = "https://localhost:25187" });

            // Initiate relay
            handler
                .RespondTo()
                .Post()
                .ForUrl("/relay/profiles/me/phone")
                .With(HttpStatusCode.OK);

            // Get profile
            handler
                .RespondTo()
                .Get()
                .ForUrl("/api/profiles/me")
                .With(HttpStatusCode.OK)
                .AndJsonContent(
                    new ZwiftProfileResponse
                    {
                        Id = 1,
                        LikelyInGame = true,
                        PublicId = "12345",
                        Riding = true,
                        WorldId = 1
                    });

            return handler;
        }
    }
}