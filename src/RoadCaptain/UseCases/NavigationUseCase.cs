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
        private readonly PlannedRoute _plannedRoute;

        public NavigationUseCase(
            IGameStateReceiver gameStateReceiver, 
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver)
        {
            _gameStateReceiver = gameStateReceiver;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;

            _plannedRoute = new PlannedRoute();
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
                    null);

            // Start listening for game state updates
            _gameStateReceiver.Start(token);

            // Block until the hosting service is stopped.
            // That cancels the CancellationToken.
            token.WaitHandle.WaitOne();
        }

        private void HandleCommandsAvailable(List<TurnDirection> commands)
        {
            if (CommandsMatchTurnsToNextSegment(commands, _plannedRoute.TurnsToNextSegment))
            {
                _monitoringEvents.Information("Executing turn {TurnDirection}", _plannedRoute.TurnToNextSegment);
                _messageReceiver.SendTurnCommand(_plannedRoute.TurnToNextSegment);
            }
            else
            {
                _monitoringEvents.Error(
                    "Expected turn commands {ExpectedTurnCommands} but instead got {TurnCommands}",
                    string.Join(", ", _plannedRoute.TurnsToNextSegment),
                    string.Join(", ", commands));
            }
        }

        private static bool CommandsMatchTurnsToNextSegment(
            List<TurnDirection> commands, 
            List<TurnDirection> turnsToNextSegemnt)
        {
            return commands.Count == turnsToNextSegemnt.Count && 
                   commands.All(turnsToNextSegemnt.Contains);
        }

        private void HandleTurnsAvailable(List<Turn> turns)
        {
        }

        private void HandleSegmentChanged(string segmentId)
        {
            // Are we already in a segment?
            if (_plannedRoute.CurrentSegment == null)
            {
                // - Check if we've dropped into the start segment
                if (segmentId == _plannedRoute.StartingSegmentId)
                {
                    _plannedRoute.EnteredSegment(segmentId);
                }
                else
                {
                    _monitoringEvents.Warning("Rider entered segment {SegmentId} but it's not the start of the route");
                }
            }
            else if(_plannedRoute.NextSegmentId == segmentId)
            {
                // We moved into the next expected segment
                _plannedRoute.EnteredSegment(segmentId);
            }
            else
            {
                _monitoringEvents.Error("Rider entered segment {SegmentId} but it's not the expected next segment on the route ({NextSegmentId})", segmentId, _plannedRoute.NextSegmentId);
            }
        }
    }
}