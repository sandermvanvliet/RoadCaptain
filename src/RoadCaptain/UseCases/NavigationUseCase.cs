using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class NavigationUseCase
    {
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private PlannedRoute _plannedRoute;
        private uint _lastSequenceNumber;

        public NavigationUseCase(
            IGameStateReceiver gameStateReceiver,
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver)
        {
            _gameStateReceiver = gameStateReceiver;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
        }

        public void Execute(CancellationToken token)
        {
            // Set up handlers
            _gameStateReceiver
                .Register(
                    null,
                    HandleSegmentChanged,
                    HandleTurnsAvailable,
                    null,
                    HandleCommandsAvailable,
                    null,
                    null, 
                    RouteSelected,
                    LastSequenceNumberUpdated);

            // Start listening for game state updates,
            // the Start() method will block until token
            // is cancelled
            _gameStateReceiver.Start(token);
        }

        private void LastSequenceNumberUpdated(uint sequenceNumber)
        {
            _lastSequenceNumber = sequenceNumber;
        }

        private void RouteSelected(PlannedRoute route)
        {
            _plannedRoute = route;
        }

        private void HandleCommandsAvailable(List<TurnDirection> commands)
        {
            if (commands.Any())
            {
                if (CommandsMatchTurnToNextSegment(commands, _plannedRoute.TurnToNextSegment))
                {
                    _monitoringEvents.Information("Executing turn {TurnDirection}", _plannedRoute.TurnToNextSegment);
                    _messageReceiver.SendTurnCommand(_plannedRoute.TurnToNextSegment, _lastSequenceNumber);
                }
                else
                {
                    _monitoringEvents.Error(
                        "Expected turn command {ExpectedTurnCommand} to be present but instead got: {TurnCommands}",
                        _plannedRoute.TurnToNextSegment,
                        string.Join(", ", commands));
                }
            }
        }

        private static bool CommandsMatchTurnToNextSegment(
            List<TurnDirection> commands,
            TurnDirection turnToNextSegemnt)
        {
            return commands.Contains(turnToNextSegemnt);
        }

        private void HandleTurnsAvailable(List<Turn> turns)
        {
        }

        private void HandleSegmentChanged(string segmentId)
        {
            // Are we already in a segment?
            if (_plannedRoute.CurrentSegmentId == null)
            {
                // - Check if we've dropped into the start segment
                if (segmentId == _plannedRoute.StartingSegmentId)
                {
                    try
                    {
                        _plannedRoute.EnteredSegment(segmentId);
                    }
                    catch (ArgumentException e)
                    {
                        _monitoringEvents.Error("Failed to enter segment because {Reason}", e.Message);
                    }
                }
                else
                {
                    _monitoringEvents.Warning("Rider entered segment {SegmentId} but it's not the start of the route", segmentId);
                }
            }
            else if (_plannedRoute.NextSegmentId == segmentId)
            {
                // We moved into the next expected segment
                try
                {
                    _plannedRoute.EnteredSegment(segmentId);
                }
                catch (ArgumentException e)
                {
                    _monitoringEvents.Error("Failed to enter segment because {Reason}", e.Message);
                }
            }
            else
            {
                _monitoringEvents.Error("Rider entered segment {SegmentId} but it's not the expected next segment on the route ({NextSegmentId})", segmentId, _plannedRoute.NextSegmentId);
            }
        }
    }
}