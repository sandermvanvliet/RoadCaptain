using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;

namespace RoadCaptain.Runner.HostedServices
{
    internal class ConnectToZwiftService : SynchronizedService
    {
        private readonly ConnectToZwiftUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Configuration _configuration;

        public ConnectToZwiftService(MonitoringEvents monitoringEvents,
            ConnectToZwiftUseCase useCase, Configuration configuration, ISynchronizer synchronizer) : base(monitoringEvents, synchronizer)
        {
            _useCase = useCase;
            _configuration = configuration;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override string Name => nameof(ConnectToZwiftService);

        protected override Task StartCoreAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _useCase.ExecuteAsync(new ConnectCommand
                    {
                        AccessToken = _configuration.ZwiftAccessToken
                    },
                    _cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        protected override Task StopCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
            }

            return Task.CompletedTask;
        }
    }
}