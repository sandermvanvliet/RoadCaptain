using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class NavigationService: SynchronizedService
    {
        private readonly NavigationUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IGameStateDispatcher _gameStateDispatcher;

        public NavigationService(MonitoringEvents monitoringEvents,
            NavigationUseCase useCase,
            IGameStateDispatcher gameStateDispatcher, ISynchronizer synchronizer)
        :base(monitoringEvents, synchronizer)
        {
            _useCase = useCase;
            _gameStateDispatcher = gameStateDispatcher;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override string Name => nameof(NavigationService);

        protected override Task StartCoreAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(
                () => _useCase.Execute(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            var route = SegmentSequenceBuilder.TestLoopOne();

            _gameStateDispatcher.RouteSelected(route);

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
