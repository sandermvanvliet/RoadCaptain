using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class ConnectToZwiftService : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ConnectToZwiftUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Configuration _configuration;

        public ConnectToZwiftService(
            MonitoringEvents monitoringEvents,
            ConnectToZwiftUseCase useCase, Configuration configuration)
        {
            _monitoringEvents = monitoringEvents;
            _useCase = useCase;
            _configuration = configuration;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _useCase.ExecuteAsync(new ConnectCommand
                    {
                        Username = _configuration.Username,
                        Password = _configuration.Password
                    },
                    _cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            _monitoringEvents.ServiceStarted(nameof(ConnectToZwiftService));

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
                _monitoringEvents.ServiceStopped(nameof(ConnectToZwiftService));
            }

            return Task.CompletedTask;
        }
    }
}