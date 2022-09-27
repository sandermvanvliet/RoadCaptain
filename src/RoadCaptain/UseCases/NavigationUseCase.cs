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
        private ulong _lastSequenceNumber;
        private int _lastRouteSequenceIndex;
        private GameState _previousState;
        private readonly IZwiftGameConnection _gameConnection;

        public NavigationUseCase(
            IGameStateReceiver gameStateReceiver,
            MonitoringEvents monitoringEvents, 
            IZwiftGameConnection gameConnection)
        {
            _gameStateReceiver = gameStateReceiver;
            _monitoringEvents = monitoringEvents;
            _gameConnection = gameConnection;
        }

        public void Execute(CancellationToken token)
        {
            // Set up handlers
            _gameStateReceiver.ReceiveGameState(GameStateUpdated);
            _gameStateReceiver.ReceiveLastSequenceNumber(LastSequenceNumberUpdated);

            // Start listening for game state updates,
            // the Start() method will block until token
            // is cancelled
            _gameStateReceiver.Start(token);
        }

        private void GameStateUpdated(GameState gameState)
        {
            if (gameState is UpcomingTurnState turnState && _previousState is not UpcomingTurnState)
            {
                var nextTurnDirection = TurnCommandFor(turnState.Directions, turnState.Route.TurnToNextSegment);

                if (CommandsMatchTurnToNextSegment(turnState.Directions, nextTurnDirection))
                {
                    _monitoringEvents.Information("Executing turn {TurnDirection} onto {SegmentId}", nextTurnDirection, turnState.Route.NextSegmentId);

                    // Behold: The Zwift Konami Code
                    //
                    // Due to a bug in Zwift the GoStraight command was ignored effectively preventing
                    // routes with a GoStraight turn on them to be followed.
                    //
                    // See https://github.com/sandermvanvliet/RoadCaptain/issues/91#issuecomment-1205613709
                    // for the Zwift dev response as to why this works.
                    if (nextTurnDirection == TurnDirection.GoStraight &&
                        turnState.Directions.Contains(TurnDirection.Left))
                    {
                        _gameConnection.SendTurnCommand(TurnDirection.Left, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.Left, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.Right, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.GoStraight, _lastSequenceNumber, gameState.RiderId);
                    }
                    else if (nextTurnDirection == TurnDirection.GoStraight &&
                        turnState.Directions.Contains(TurnDirection.Right))
                    {
                        _gameConnection.SendTurnCommand(TurnDirection.Right, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.Right, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.Left, _lastSequenceNumber, gameState.RiderId);
                        _gameConnection.SendTurnCommand(TurnDirection.GoStraight, _lastSequenceNumber, gameState.RiderId);
                    }
                    else
                    {
                        _gameConnection.SendTurnCommand(nextTurnDirection, _lastSequenceNumber, gameState.RiderId);
                    }
                }
                else
                {
                    _monitoringEvents.Error(
                        "Expected turn command {ExpectedTurnCommand} to be present but instead got: {TurnCommands}",
                        nextTurnDirection,
                        string.Join(", ", turnState.Directions));
                }
            }

            if (gameState is OnRouteState routeState)
            {
                if (routeState.Route.CurrentSegmentSequence.Index != _lastRouteSequenceIndex)
                {
                    _monitoringEvents.Information(
                        "Entered route segment {CurrentSegment} ({CurrentIndex})",
                        routeState.Route.CurrentSegmentSequence.SegmentId,
                        routeState.Route.CurrentSegmentSequence.Index);
                }

                _lastRouteSequenceIndex = routeState.Route.CurrentSegmentSequence.Index;
            }

            _previousState = gameState;
        }

        private void LastSequenceNumberUpdated(ulong sequenceNumber)
        {
            _lastSequenceNumber = sequenceNumber;
        }

        private static bool CommandsMatchTurnToNextSegment(
            List<TurnDirection> commands,
            TurnDirection turnToNextSegment)
        {
            return commands.Contains(turnToNextSegment);
        }

        internal static TurnDirection TurnCommandFor(List<TurnDirection> commands, TurnDirection nextTurn)
        {
            if (nextTurn == TurnDirection.Left)
            {
                if (commands.Contains(TurnDirection.Left))
                {
                    if (commands.Contains(TurnDirection.GoStraight) ||
                        commands.Contains(TurnDirection.Right))
                    {
                        return TurnDirection.Left;
                    }
                }

                return TurnDirection.GoStraight;
            }

            if (nextTurn == TurnDirection.GoStraight)
            {
                if (commands.Contains(TurnDirection.Right))
                {
                    if (commands.Contains(TurnDirection.GoStraight))
                    {
                        return TurnDirection.GoStraight;
                    }

                    // Other available command is Left
                    return TurnDirection.Right;
                }

                if (commands.Contains(TurnDirection.Left))
                {
                    return TurnDirection.GoStraight;
                }
            }

            if (nextTurn == TurnDirection.Right)
            {
                if (commands.Contains(TurnDirection.Right))
                {
                    if (commands.Contains(TurnDirection.GoStraight) ||
                        commands.Contains(TurnDirection.Left))
                    {
                        return TurnDirection.Right;
                    }
                }
                
                return TurnDirection.GoStraight;
            }

            return TurnDirection.None;
        }
    }
}