// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

        private List<Segment>? _segments;
        private readonly ISegmentStore _segmentStore;
        private PlannedRoute? _route;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private static ulong _lastIncomingSequenceNumber;
        private readonly IZwiftGameConnection _gameConnection;
        private readonly IGameStateReceiver _gameStateReceiver;
        private GameState? _previousGameState;

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
            _gameStateReceiver.ReceiveRoute(route => _route = route);

            _gameStateReceiver.ReceiveGameState(gameState =>
            {
                if(_previousGameState is WaitingForConnectionState && gameState is ConnectedToZwiftState && _pingedBefore)
                {
                    // This resets the pingedBefore flag so that when
                    // we lose the connection and regain it, the pairing
                    // message is sent again.
                    _pingedBefore = false;
                }

                _previousGameState = gameState;
            });
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

                try
                {
                    if (message is ZwiftRiderPositionMessage riderPosition)
                    {
                        _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);

                        // TODO: Figure out how to get the WorldId as quickly as possible and put it in the game state
                        var worldId = _route?.World?.ZwiftId ?? ZwiftWorldId.Unknown;

                        // Convert from Zwift game coordinates to a lat/lon coordinate
                        var position = new GameCoordinate(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude, worldId).ToTrackPoint();

                        // As long as there is no route loaded we cannot change the
                        // the state.
                        if (_route is { World: { } })
                        {
                            _segments ??= _segmentStore.LoadSegments(_route.World, _route.Sport);

                            _gameStateDispatcher.UpdatePosition(position, _segments, _route);
                        }
                    }
                    else if (message is ZwiftPingMessage ping)
                    {
                        HandlePingMessage(ping);
                    }
                    else if (message is ZwiftCommandAvailableMessage commandAvailable)
                    {
                        HandleAvailableTurns(commandAvailable);
                    }
                    else if (message is ZwiftActivityDetailsMessage activityDetails)
                    {
                        if (activityDetails.ActivityId != 0)
                        {
                            _gameStateDispatcher.EnterGame(activityDetails.RiderId, activityDetails.ActivityId);
                        }
                        else
                        {
                            _gameStateDispatcher.LeaveGame();
                        }
                    }
                }
                catch (InvalidStateTransitionException ex)
                {
                    _monitoringEvents.InvalidStateTransition(ex);
                }
            }
        }

        private void HandleAvailableTurns(ZwiftCommandAvailableMessage commandAvailable)
        {
            if ("somethingempty".Equals(commandAvailable.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                DispatchLastSequenceNumber(commandAvailable);
            }
            else 
            {
                _monitoringEvents.Debug("Received command type {Type}", commandAvailable.Type);
                _gameStateDispatcher.TurnCommandAvailable(commandAvailable.Type);
            }
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
