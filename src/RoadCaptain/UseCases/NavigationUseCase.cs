using System.Collections.Generic;
using System.Threading;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class NavigationUseCase
    {
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private ulong _lastSequenceNumber;
        private int _lastRouteSequenceIndex;
        private GameState _previousState;

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
                    LastSequenceNumberUpdated,
                    GameStateUpdated);

            // Start listening for game state updates,
            // the Start() method will block until token
            // is cancelled
            _gameStateReceiver.Start(token);
        }

        private void GameStateUpdated(GameState gameState)
        {
            if (gameState is UpcomingTurnState turnState && _previousState is not UpcomingTurnState)
            {
                if (CommandsMatchTurnToNextSegment(turnState.Directions, turnState.Route.TurnToNextSegment))
                {
                    _monitoringEvents.Information("Executing turn {TurnDirection}", turnState.Route.TurnToNextSegment);
                    _messageReceiver.SendTurnCommand(turnState.Route.TurnToNextSegment, _lastSequenceNumber);
                }
                else
                {
                    _monitoringEvents.Error(
                        "Expected turn command {ExpectedTurnCommand} to be present but instead got: {TurnCommands}",
                        turnState.Route.TurnToNextSegment,
                        string.Join(", ", turnState.Directions));
                }
            }

            if (gameState is OnRouteState routeState)
            {
                if (routeState.Route.SegmentSequenceIndex != _lastRouteSequenceIndex)
                {
                    _monitoringEvents.Information(
                        "Moved to {CurrentSegment} ({CurrentIndex})",
                        routeState.Route.CurrentSegmentId,
                        routeState.Route.SegmentSequenceIndex);
                }

                _lastRouteSequenceIndex = routeState.Route.SegmentSequenceIndex;
            }

            _previousState = gameState;
        }

        private void LastSequenceNumberUpdated(ulong sequenceNumber)
        {
            _lastSequenceNumber = sequenceNumber;
        }

        private static bool CommandsMatchTurnToNextSegment(
            List<TurnDirection> commands,
            TurnDirection turnToNextSegemnt)
        {
            return commands.Contains(turnToNextSegemnt);
        }
    }
}