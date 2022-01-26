using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Hosting;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console
{
    internal class RoadCaptainConsoleHost : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IContainer _container;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public RoadCaptainConsoleHost()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MonitoringEventsWithSerilog>().As<MonitoringEvents>();
            builder.RegisterModule<DomainModule>();

            _container = builder.Build();

            _monitoringEvents = _container.Resolve<MonitoringEvents>();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _monitoringEvents.ApplicationStarted();

            var incomingMessageUseCase = _container.Resolve<HandleIncomingMessageUseCase>();

            Task.Factory.StartNew(() => incomingMessageUseCase.Execute(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
            }

            _monitoringEvents.ApplicationEnded();

            return Task.CompletedTask;
        }
    }
}