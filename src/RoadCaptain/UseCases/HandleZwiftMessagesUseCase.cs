using System.Collections.Generic;
using System.Threading;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleZwiftMessagesUseCase
    {
        private readonly IMessageEmitter _emitter;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private bool _pingedBefore;
        private static readonly object SyncRoot = new();
        private readonly HandleAvailableTurnsUseCase _handleAvailableTurnsUseCase;
        private readonly HandleActivityDetailsUseCase _handleActivityDetailsUseCase;

        private GameState _gameState = new NotInGameState();
        private List<Segment> _segments;
        private readonly ISegmentStore _segmentStore;
        private readonly PlannedRoute _route;
        private readonly IGameStateDispatcher _gameStateDispatcher;

        public HandleZwiftMessagesUseCase(
            IMessageEmitter emitter,
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver, 
            HandleAvailableTurnsUseCase handleAvailableTurnsUseCase, 
            HandleActivityDetailsUseCase handleActivityDetailsUseCase, 
            ISegmentStore segmentStore, 
            IGameStateDispatcher gameStateDispatcher)
        {
            _emitter = emitter;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
            _handleAvailableTurnsUseCase = handleAvailableTurnsUseCase;
            _handleActivityDetailsUseCase = handleActivityDetailsUseCase;
            _segmentStore = segmentStore;
            _gameStateDispatcher = gameStateDispatcher;

            _route = new SegmentSequenceBuilder()
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
                .GoingStraightTo("watopia-big-loop-rev-001-after-after")
                .EndingAt("watopia-big-loop-rev-001-after-after")
                .Build();
        }

        public GameState State
        {
            get => _gameState;
            set
            {
                if (_gameState.GetType() != value.GetType())
                {
                    _monitoringEvents.Information("Game state changed from {OldState} to {NewState}", _gameState.GetType().Name, value.GetType().Name);
                }

                _gameState = value;

                _gameStateDispatcher.Dispatch(_gameState);
            }
        }

        public void Execute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Dequeue will block if there are no messages in the queue
                var message = _emitter.Dequeue(token);

                if (message is ZwiftRiderPositionMessage riderPosition)
                {
                    _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);

                    // Convert from Zwift game coordinates to a lat/lon coordinate
                    var position = TrackPoint.FromGameLocation((decimal)riderPosition.Latitude, (decimal)riderPosition.Longitude, (decimal)riderPosition.Altitude);

                    if (_segments == null)
                    {
                        _segments = _segmentStore.LoadSegments();
                    }

                    State = State.UpdatePosition(position, _segments, _route);
                }
                else if (message is ZwiftPingMessage ping)
                {
                    HandlePingMessage(ping);
                }
                else if (message is ZwiftCommandAvailableMessage commandAvailable)
                {
                    //_handleAvailableTurnsUseCase.Execute(commandAvailable);
                }
                else if (message is ZwiftActivityDetailsMessage activityDetails)
                {
                    if (activityDetails.ActivityId != 0)
                    {
                        State = State.EnterGame(activityDetails.ActivityId);
                    }
                    else
                    {
                        State = State.LeaveGame();
                    }
                }
            }
        }

        private void HandlePingMessage(ZwiftPingMessage ping)
        {
            if (!_pingedBefore)
            {
                lock (SyncRoot)
                {
                    if (_pingedBefore)
                    {
                        return;
                    }

                    _pingedBefore = true;
                }

                _messageReceiver.SendInitialPairingMessage(ping.RiderId, 0);
            }
        }
    }
}