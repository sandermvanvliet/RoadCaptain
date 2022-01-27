using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class HandleZwiftMessagesService : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly HandleZwiftMessagesUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public HandleZwiftMessagesService(
            MonitoringEvents monitoringEvents,
            HandleZwiftMessagesUseCase useCase)
        {
            _monitoringEvents = monitoringEvents;
            _useCase = useCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(
                () => _useCase.Execute(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            _monitoringEvents.ServiceStarted(nameof(HandleZwiftMessagesService));

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
            finally
            {
                _monitoringEvents.ServiceStopped(nameof(HandleZwiftMessagesService));
            }

            return Task.CompletedTask;
        }
    }
}