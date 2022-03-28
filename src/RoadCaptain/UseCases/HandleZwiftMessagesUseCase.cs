using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleZwiftMessagesUseCase
    {
        private readonly IMessageEmitter _emitter;
        private readonly MonitoringEvents _monitoringEvents;
        private bool _pingedBefore;
        private static readonly object SyncRoot = new();

        private GameState _gameState;
        private List<Segment> _segments;
        private readonly ISegmentStore _segmentStore;
        private PlannedRoute _route;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private static ulong _lastIncomingSequenceNumber;
        private readonly IZwiftGameConnection _gameConnection;
        private readonly IGameStateReceiver _gameStateReceiver;

        public HandleZwiftMessagesUseCase(
            IMessageEmitter emitter,
            MonitoringEvents monitoringEvents, 
            ISegmentStore segmentStore, 
            IGameStateDispatcher gameStateDispatcher, 
            IZwiftGameConnection gameConnection,
            IGameStateReceiver gameStateReceiver)
        {
            _emitter = emitter;
            _monitoringEvents = monitoringEvents;
            _segmentStore = segmentStore;
            _gameStateDispatcher = gameStateDispatcher;
            _gameConnection = gameConnection;
            _gameStateReceiver = gameStateReceiver;

            // The route is needed to update game state,
            // so this use case needs to listen to RouteSelected
            // updates.
            _gameStateReceiver.Register(route => _route = route, null, null);
        }

        public GameState State
        {
            get => _gameState;
            set
            {
                if (ReferenceEquals(_gameState, value))
                {
                    return;
                }

                if (_gameState != null && _gameState.GetType() != value.GetType())
                {
                    _monitoringEvents.Information("Game state changed from {OldState} to {NewState}", _gameState.GetType().Name, value.GetType().Name);
                }

                _gameState = value;

                _gameStateDispatcher.Dispatch(_gameState);
            }
        }

        public void Execute(CancellationToken token)
        {
            // Typically the game state receiver has already been started
            // and this task exists immediately.
            Task.Factory.StartNew(() => _gameStateReceiver.Start(token));

            while (!token.IsCancellationRequested)
            {
                // Dequeue will block if there are no messages in the queue
                var message = _emitter.Dequeue(token);

                if (message is ZwiftRiderPositionMessage riderPosition)
                {
                    _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);

                    // Convert from Zwift game coordinates to a lat/lon coordinate
                    var position = TrackPoint.FromGameLocation(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);

                    if (_segments == null)
                    {
                        _segments = _segmentStore.LoadSegments();
                    }

                    // As long as there is no route loaded we cannot change the
                    // the state.
                    if (_route != null)
                    {
                        State = State.UpdatePosition(position, _segments, _route);
                    }
                }
                else if (message is ZwiftPingMessage ping)
                {
                    HandlePingMessage(ping);
                }
                else if (message is ZwiftCommandAvailableMessage commandAvailable)
                {
                    State = HandleAvailableTurns(commandAvailable, State);
                }
                else if (message is ZwiftActivityDetailsMessage activityDetails)
                {
                    if (activityDetails.ActivityId != 0)
                    {
                        State = State.EnterGame(activityDetails.RiderId, activityDetails.ActivityId);
                    }
                    else
                    {
                        State = State.LeaveGame();
                    }
                }
            }
        }

        private GameState HandleAvailableTurns(ZwiftCommandAvailableMessage commandAvailable, GameState state)
        {
            if ("somethingempty".Equals(commandAvailable.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                DispatchLastSequenceNumber(commandAvailable);

                return state;
            }

            if (state is OnRouteState routeState)
            {
                return routeState.TurnCommandAvailable(commandAvailable.Type);
            }

            return state;
        }

        private void DispatchLastSequenceNumber(ZwiftCommandAvailableMessage commandAvailable)
        {
            if (commandAvailable.SequenceNumber > _lastIncomingSequenceNumber)
            {
                // Take new sequence number from here as the "SomethingEmpty"
                // appears to be a synchronization mechanism
                _lastIncomingSequenceNumber = commandAvailable.SequenceNumber;
                _gameStateDispatcher.UpdateLastSequenceNumber(commandAvailable.SequenceNumber);
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

                _gameConnection.SendInitialPairingMessage(ping.RiderId, 0);
            }
        }
    }
}