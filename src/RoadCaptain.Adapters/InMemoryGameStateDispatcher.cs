using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class InMemoryGameStateDispatcher : IGameStateDispatcher, IGameStateReceiver
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ConcurrentQueue<Message> _queue;
        
        private readonly List<Action<SegmentDirection>> _directionChangedHandlers = new();
        private readonly List<Action<List<TurnDirection>>> _turnCommandsAvailableHandlers = new();
        private readonly List<Action<List<Turn>>> _turnsAvailableHandlers = new();
        private readonly List<Action<PlannedRoute>> _routeSelectedHandlers = new();
        private readonly List<Action<uint>> _lastSequenceNumberHandlers = new();
        private readonly List<Action<GameState>> _gameStateHandlers = new();
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly TimeSpan _queueWaitTimeout = TimeSpan.FromMilliseconds(2000);
        private bool _started;
        private static readonly object SyncRoot = new();
        private GameState _gameState;

        public InMemoryGameStateDispatcher(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _queue = new ConcurrentQueue<Message>();
        }

        public TrackPoint CurrentPosition
        {
            get
            {
                if (_gameState is PositionedState positionedState)
                {
                    return positionedState.CurrentPosition;
                }

                return null;
            }
        }

        public Segment CurrentSegment  
        {
            get
            {
                if (_gameState is OnSegmentState segmentState)
                {
                    return segmentState.CurrentSegment;
                }

                return null;
            }
        }

        public List<Turn> AvailableTurns { get; private set; } = new();

        public SegmentDirection CurrentDirection { get; private set; } = SegmentDirection.Unknown;

        public ulong LastSequenceNumber { get; private set; }

        public List<TurnDirection> AvailableTurnCommands { get; private set; } = new();

        public bool InGame { get; private set; }

        private bool Started
        {
            get => _started;
            set
            {
                lock (SyncRoot)
                {
                    _started = value;
                }
            }
        }

        public void TurnsAvailable(List<Turn> turns)
        {
            if (!InGame)
            {
                return;
            }

            if (turns.Any())
            {
                _monitoringEvents.Information("Upcoming turns: ");

                foreach (var turn in turns)
                {
                    _monitoringEvents.Information("{Direction} onto {Segment}", turn.Direction,
                        turn.SegmentId);
                }
            }

            AvailableTurns = turns;
            Enqueue("turnsAvailable", AvailableTurns);
        }

        public void DirectionChanged(SegmentDirection direction)
        {
            if (!InGame)
            {
                return;
            }

            if (direction != SegmentDirection.Unknown)
            {
                var formattedDirection = FormatDirection(direction);

                _monitoringEvents.Information("Direction is now {Direction}", formattedDirection);

                var turns = CurrentSegment.NextSegments(direction);

                // Only show turns if we have actual options.
                if (turns.Any(t => t.Direction != TurnDirection.GoStraight))
                {
                    TurnsAvailable(turns);
                }
            }
            else if(AvailableTurns.Any())
            {
                // If we don't have a direction then we also don't
                // know which turns are available.
                TurnsAvailable(new List<Turn>());
            }

            CurrentDirection = direction;
            Enqueue("directionChanged", CurrentDirection);
        }

        private static string FormatDirection(SegmentDirection direction)
        {
            return direction switch
            {
                SegmentDirection.AtoB => "A -> B",
                SegmentDirection.BtoA => "B -> A",
                _ => "Unknown"
            };
        }

        public void TurnCommandsAvailable(List<TurnDirection> turns)
        {
            AvailableTurnCommands = turns;
            Enqueue("turnCommandsAvailable", AvailableTurnCommands);
        }

        public void RouteSelected(PlannedRoute route)
        {
            Enqueue("routeSelected", route);
        }

        public void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            if (sequenceNumber > LastSequenceNumber)
            {
                LastSequenceNumber = sequenceNumber;
                Enqueue("lastSequenceNumber", sequenceNumber);
            }
        }

        public void RouteStarted()
        {
            Enqueue("routeStarted", "");
        }

        public void RouteProgression(int step, string segmentId)
        {
            Enqueue("routeProgression", step);
        }

        public void RouteCompleted()
        {
            Enqueue("routeCompleted", "");
        }

        public void Dispatch(GameState gameState)
        {
            if (gameState is InGameState)
            {
                InGame = true;
            }
            else
            {
                InGame = false;
            }

            _gameState = gameState;

            Enqueue("gameState", gameState);
        }

        protected virtual void Enqueue(string topic, object data)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    TimeStamp = DateTime.UtcNow,
                    Type = data.GetType(),
                    Data = JsonConvert.SerializeObject(data)
                };

                _queue.Enqueue(message);
            }
            catch (Exception ex)
            {
                _monitoringEvents.Error(ex, "Failed to dispatch game state update");
            }
            finally
            {
                // Unblock the Dequeue method
                _autoResetEvent.Set();
            }
        }

        public void Start(CancellationToken token)
        {
            // Only have one running at a time,
            // it's possible that this call is 
            // made from various consumers and
            // that would cause threading issues
            // becuase we'd be dequeueing messages
            // from multiple threads.
            if (Started)
            {
                return;
            }

            Started = true;

            try
            {
                do
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        InvokeHandlers(message);
                    }
                    else
                    {
                        // Only wait if nothing is in the queue,
                        // otherwise loop aroud and take the next
                        // item from the queue without waiting.
                        _autoResetEvent.WaitOne(_queueWaitTimeout);
                    }
                } while (!token.IsCancellationRequested);
            }
            finally
            {
                Started = false;
            }
        }

        public void Register(Action<List<Turn>> turnsAvailable, Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable,
            Action<PlannedRoute> routeSelected, Action<uint> lastSequenceNumber, Action<GameState> gameState)
        {
            AddHandlerIfNotNull(_turnsAvailableHandlers, turnsAvailable);
            AddHandlerIfNotNull(_directionChangedHandlers, directionChanged);
            AddHandlerIfNotNull(_turnCommandsAvailableHandlers, turnCommandsAvailable);
            AddHandlerIfNotNull(_routeSelectedHandlers, routeSelected);
            AddHandlerIfNotNull(_lastSequenceNumberHandlers, lastSequenceNumber);
            AddHandlerIfNotNull(_gameStateHandlers, gameState);
        }

        private static void AddHandlerIfNotNull<TMessage>(List<Action<TMessage>> collection, Action<TMessage> handler)
        {
            if (handler != null)
            {
                collection.Add(handler);
            }
        }

        private void InvokeHandlers(Message message)
        {
            if (message == null)
            {
                return;
            }

            switch (message.Topic)
            {
                case "turnsAvailable":
                    _turnsAvailableHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
                case "directionChanged":
                    _directionChangedHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
                case "turnCommandsAvailable":
                    _turnCommandsAvailableHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
                case "routeSelected":
                    _routeSelectedHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
                case "lastSequenceNumber":
                    _lastSequenceNumberHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
                case "gameState":
                    _gameStateHandlers.ForEach(h => InvokeHandler(h, message.Data, message.Type));
                    break;
            }
        }

        private void InvokeHandler<TMessage>(Action<TMessage> handle, string serializedContent, Type payloadType)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject(serializedContent, payloadType);

                handle.DynamicInvoke(payload);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to invoke handler");
            }
        }
    }
}