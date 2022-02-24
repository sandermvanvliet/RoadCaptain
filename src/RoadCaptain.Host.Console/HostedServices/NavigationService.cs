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
            
            var route = new SegmentSequenceBuilder()
                .StartingAt("watopia-bambino-fondo-001-after-after-after-after-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 1
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 2
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 3
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TurningLeftTo("watopia-beach-island-loop-004")
                .TurningLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .TurningRightTo("watopia-bambino-fondo-004-before-before")
                // Around the volcano
                .TurningRightTo("watopia-bambino-fondo-004-before-after")
                .TurningRightTo("watopia-beach-island-loop-001")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .TurningRightTo("watopia-bambino-fondo-001-after-after-before-after")
                // Start the cliffside loop
                .TurningRightTo("watopia-bambino-fondo-003-before-before")
                .TurningLeftTo("watopia-big-loop-rev-001-before-before")
                .TurningLeftTo("watopia-ocean-lava-cliffside-loop-001")
                .TurningLeftTo("watopia-big-loop-rev-001-after-after")
                .EndingAt("watopia-big-loop-rev-001-after-after")
                .Build();

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
