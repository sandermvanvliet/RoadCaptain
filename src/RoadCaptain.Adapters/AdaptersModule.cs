// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Net;
using System.Net.Http;
using Autofac;
using Codenizer.HttpClient.Testable;
using Microsoft.Extensions.Configuration;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public class AdaptersModule : Module
    {
        private readonly IConfiguration _configuration;

        public AdaptersModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        /// <summary>
        ///     Where to read packets from. Can be 'socket' or 'file'
        /// </summary>
        /// <remarks>When set to 'file' <see cref="CaptureFilePath" /> must be set</remarks>
        public string MessageReceiverSource { get; set; } = "socket";

        /// <summary>
        /// The path to the NPCAP file containing the TCP packets to replay
        /// </summary>
        public string? CaptureFilePath { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces()
                .Except<SecureZwiftConnection>()
                .Except<MessageReceiverFromCaptureFile>()
                .Except<MessageEmitterToQueue>()
                .Except<IGameStateDispatcher>()
                .Except<IGameStateReceiver>()
                .Except<IZwiftGameConnection>()
                .Except<IZwiftCrypto>()
                .Except<HttpRouteRepository>()
                .Except<LocalDirectoryRouteRepository>();

            builder.RegisterType<MessageEmitterConfiguration>().AsSelf();
            builder.RegisterType<ZwiftCrypto>().As<IZwiftCrypto>().SingleInstance();

            if ("socket".Equals(MessageReceiverSource, StringComparison.InvariantCultureIgnoreCase))
            {
                builder
                    .RegisterType<SecureZwiftConnection>()
                    .As<IMessageReceiver>()
                    .As<IZwiftGameConnection>()
                    .SingleInstance();
                
                builder
                    .Register(_ => new HttpClient())
                    .InstancePerLifetimeScope();
            }
            else if ("file".Equals(MessageReceiverSource, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(CaptureFilePath))
                {
                    throw new ArgumentException("CaptureFilePath is empty");
                }

                builder
                    .RegisterType<MessageReceiverFromCaptureFile>()
                    .As<IMessageReceiver>()
                    .WithParameter("captureFilePath", CaptureFilePath)
                    .SingleInstance();

                builder
                    .RegisterType<NopZwiftGameConnection>()
                    .As<IZwiftGameConnection>();

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

            builder
                .RegisterType<InMemoryGameStateDispatcher>()
                .As<IGameStateDispatcher>()
                .As<IGameStateReceiver>()
                .SingleInstance();

            builder
                .RegisterType<MessageEmitterToQueue>()
                .As<IMessageEmitter>()
                .SingleInstance();

            RegisterRouteRepositories(builder);

            builder.RegisterType<RouteStoreToDisk>().AsSelf();
        }

        private void RegisterRouteRepositories(ContainerBuilder builder)
        {
            var section = _configuration.GetSection("RouteRepositories");

            if (section == null || !section.Exists())
            {
                return;
            }

            foreach (var childSection in section.GetChildren())
            {
                var repositoryType = childSection["type"];
                if (string.IsNullOrEmpty(repositoryType))
                {
                    continue;
                }

                if ("local".Equals(repositoryType, StringComparison.InvariantCultureIgnoreCase))
                {
                    var settings = new LocalDirectoryRouteRepositorySettings(childSection);
                    builder
                        .Register<IRouteRepository>(componentContext => new LocalDirectoryRouteRepository(settings, componentContext.Resolve<MonitoringEvents>(), componentContext.Resolve<RouteStoreToDisk>()))
                        .As<IRouteRepository>()
                        .SingleInstance();
                }
                else if ("http".Equals(repositoryType, StringComparison.InvariantCultureIgnoreCase))
                {
                    var settings = new HttpRouteRepositorySettings(childSection);
                    builder
                        .Register<IRouteRepository>(componentContext => new HttpRouteRepository(new HttpClient(), settings, componentContext.Resolve<RouteStoreToDisk>()))
                        .As<IRouteRepository>()
                        .SingleInstance();
                }
            }
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
                .Put()
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
