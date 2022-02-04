using System.Collections.Generic;
using System.Linq;
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

        public List<Turn> AvailableTurns { get; private set; }

        public SegmentDirection CurrentDirection { get; private set; }

        public void PositionChanged(TrackPoint position)
        {
            CurrentPosition = position;
        }

        public void SegmentChanged(Segment segment)
        {
            if (CurrentSegment == null)
            {
                _monitoringEvents.Information("Starting in {Segment}", segment.Id);
            }
            else
            {
                _monitoringEvents.Information("Moved from {CurrentSegment} to {NewSegment}", CurrentSegment?.Id, segment.Id);
            }

            CurrentSegment = segment;
        }

        public void TurnsAvailable(List<Turn> turns)
        {
            AvailableTurns = turns;
        }

        public void DirectionChanged(SegmentDirection direction)
        {
            if (CurrentDirection != direction)
            {
                _monitoringEvents.Information("Direction is now {Direction}", direction);

                var turns = CurrentSegment.NextSegments(direction);
                            
                // Only show turns if we have actual options.
                if (turns.Any(t => t.Direction != TurnDirection.StraightOn))
                {
                    _monitoringEvents.Information("Upcoming turns: ");

                    foreach (var turn in turns)
                    {
                        _monitoringEvents.Information("{Direction} onto {Segment}", turn.Direction,
                            turn.SegmentId);
                    }
                }
            }

            CurrentDirection = direction;
        }
    }
}