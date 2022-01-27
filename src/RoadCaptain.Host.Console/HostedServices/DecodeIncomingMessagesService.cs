using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class DecodeIncomingMessagesService : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DecodeIncomingMessagesUseCase _incomingMessagesUseCase;

        public DecodeIncomingMessagesService(
            MonitoringEvents monitoringEvents,
            DecodeIncomingMessagesUseCase incomingMessagesUseCase)
        {
            _monitoringEvents = monitoringEvents;
            _incomingMessagesUseCase = incomingMessagesUseCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _incomingMessagesUseCase.ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _monitoringEvents.ServiceStarted(nameof(DecodeIncomingMessagesService));

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
                _monitoringEvents.ServiceStopped(nameof(DecodeIncomingMessagesService));
            }

            return Task.CompletedTask;
        }
    }
}