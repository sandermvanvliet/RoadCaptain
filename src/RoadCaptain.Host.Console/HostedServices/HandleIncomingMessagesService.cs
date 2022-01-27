using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class HandleIncomingMessagesService : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HandleIncomingMessageUseCase _incomingMessageUseCase;

        public HandleIncomingMessagesService(
            MonitoringEvents monitoringEvents,
            HandleIncomingMessageUseCase incomingMessageUseCase)
        {
            _monitoringEvents = monitoringEvents;
            _incomingMessageUseCase = incomingMessageUseCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _incomingMessageUseCase.ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _monitoringEvents.ServiceStarted(nameof(HandleIncomingMessagesService));

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
                _monitoringEvents.ServiceStopped(nameof(HandleIncomingMessagesService));
            }

            return Task.CompletedTask;
        }
    }
}