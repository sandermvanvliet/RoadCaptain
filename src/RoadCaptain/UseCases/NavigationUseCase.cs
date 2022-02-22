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
        private ulong _lastSequenceNumber;
        private readonly IGameStateDispatcher _dispatcher;

        public NavigationUseCase(
            IGameStateReceiver gameStateReceiver,
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver, 
            IGameStateDispatcher dispatcher)
        {
            _gameStateReceiver = gameStateReceiver;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
            _dispatcher = dispatcher;
        }

        public void Execute(CancellationToken token)
        {
            // Set up handlers
            _gameStateReceiver
                .Register(null,
                    null,
                    HandleCommandsAvailable, 
                    RouteSelected,
                    LastSequenceNumberUpdated, 
                    null);

            // Start listening for game state updates,
            // the Start() method will block until token
            // is cancelled
            _gameStateReceiver.Start(token);
        }

        private void HandleEnteredGame(ulong obj)
        {
            // Reset the route when the user enters the game
            _plannedRoute.Reset();
        }

        private void LastSequenceNumberUpdated(ulong sequenceNumber)
        {
            _lastSequenceNumber = sequenceNumber;
        }

        private void RouteSelected(PlannedRoute route)
        {
            _plannedRoute = route;
        }

        private void HandleCommandsAvailable(List<TurnDirection> commands)
        {
            if (!_plannedRoute.HasCompleted && !_plannedRoute.HasStarted)
            {
                return;
            }

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

        private void HandleSegmentChanged(string segmentId)
        {
            // Ignore empty segment and pretend nothing happened
            if (segmentId == null)
            {
                return;
            }

            // Are we already in a segment?
            if (_plannedRoute.CurrentSegmentId == null)
            {
                // - Check if we've dropped into the start segment
                if (segmentId == _plannedRoute.StartingSegmentId)
                {
                    try
                    {
                        var result = _plannedRoute.EnteredSegment(segmentId);

                        DispatchRouteState(result);
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
                    if (_plannedRoute.HasCompleted)
                    {
                        _monitoringEvents.Information("Route has completed, reverting to free-roam mode.");
                        return;
                    }

                    var result = _plannedRoute.EnteredSegment(segmentId);

                    DispatchRouteState(result);

                    if (_plannedRoute.HasCompleted)
                    {
                        _monitoringEvents.Information("Entered the final segment of the route!");
                    }
                    else
                    {
                        _monitoringEvents.Information(
                            "On segment {Step} of {TotalSteps}. Next turn will be {Turn} onto {SegmentId}",
                            _plannedRoute.SegmentSequenceIndex,
                            _plannedRoute.RouteSegmentSequence.Count,
                            _plannedRoute.TurnToNextSegment,
                            _plannedRoute.NextSegmentId
                        );
                    }
                }
                catch (ArgumentException e)
                {
                    _monitoringEvents.Error("Failed to enter segment because {Reason}", e.Message);
                }
            }
            else
            {
                _monitoringEvents.Warning(
                    "Rider entered segment {SegmentId} but it's not the expected next segment on the route ({NextSegmentId})",
                    segmentId,
                    _plannedRoute.NextSegmentId);
            }
        }

        private void DispatchRouteState(RouteMoveResult result)
        {
            if (result == RouteMoveResult.StartedRoute)
            {
                _dispatcher.RouteStarted();
            }
            else if (result == RouteMoveResult.EnteredNextSegment)
            {
                _dispatcher.RouteProgression(_plannedRoute.SegmentSequenceIndex, _plannedRoute.CurrentSegmentId);
            }
            else if (result == RouteMoveResult.CompletedRoute)
            {
                _dispatcher.RouteCompleted();
            }
        }
    }
}