using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class InMemoryGameStateDispatcher : IGameStateDispatcher
    {
        private readonly MonitoringEvents _monitoringEvents;

        public InMemoryGameStateDispatcher(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }

        public TrackPoint CurrentPosition { get; private set; }

        public Segment CurrentSegment { get; private set; }

        public List<Turn> AvailableTurns { get; private set; } = new();

        public SegmentDirection CurrentDirection { get; private set; } = SegmentDirection.Unknown;

        public List<TurnDirection> AvailableTurnCommands { get; private set; } = new();

        public bool InGame { get; private set; }

        public void PositionChanged(TrackPoint position)
        {
            if (InGame)
            {
                CurrentPosition = position;

                Enqueue("positionChanged", CurrentPosition);
            }
        }

        public void SegmentChanged(Segment segment)
        {
            if(segment != null)
            {
                if (CurrentSegment == null)
                {
                    _monitoringEvents.Information("Starting in {Segment}", segment.Id);
                }
                else
                {
                    _monitoringEvents.Information("Moved from {CurrentSegment} to {NewSegment}", CurrentSegment?.Id,
                        segment.Id);
                }
            }
            else if (CurrentSegment != null)
            {
                _monitoringEvents.Warning("Lost segment lock for rider");
            }

            CurrentSegment = segment;
            Enqueue("segmentChanged", CurrentSegment?.Id);

            // TODO: clear available turns, available turn commands and direction (although direction follows very quickly after)
            // This can most likely be removed here and handled by the SoemthingEmpty
            // command we receive from Zwift.
            TurnCommandsAvailable(new List<TurnDirection>());
        }

        public void TurnsAvailable(List<Turn> turns)
        {
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
            if (direction != SegmentDirection.Unknown)
            {
                _monitoringEvents.Information("Direction is now {Direction}", direction);

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

        public void TurnCommandsAvailable(List<TurnDirection> turns)
        {
            AvailableTurnCommands = turns;
            Enqueue("turnCommandsAvailable", AvailableTurnCommands);
        }

        public void EnterGame()
        {
            InGame = true;
            Enqueue("inGame", InGame);
        }

        public void LeaveGame()
        {
            InGame = false;
            Enqueue("inGame", InGame);
        }

        protected virtual void Enqueue(string topic, object data)
        {
        }
    }
}