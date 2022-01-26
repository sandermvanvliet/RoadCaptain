using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console
{
    internal class RoadCaptainConsoleHost : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HandleIncomingMessageUseCase _incomingMessageUseCase;

        public RoadCaptainConsoleHost(MonitoringEvents monitoringEvents, HandleIncomingMessageUseCase incomingMessageUseCase)
        {
            _monitoringEvents = monitoringEvents;
            _incomingMessageUseCase = incomingMessageUseCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _monitoringEvents.ApplicationStarted();
            
            Task.Factory.StartNew(() => _incomingMessageUseCase.Execute(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            
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