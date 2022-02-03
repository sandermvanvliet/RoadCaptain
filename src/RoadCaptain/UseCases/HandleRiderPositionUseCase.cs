using System.Collections.Generic;
using System.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleRiderPositionUseCase
    {
        private List<Segment> _segments;
        private Segment _currentSegment;
        private TrackPoint _previousPositionOnSegment;
        private SegmentDirection _currentDirection;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ISegmentStore _segmentStore;
        private readonly List<TurnDirection> _availableTurnCommands = new();

        public HandleRiderPositionUseCase(MonitoringEvents monitoringEvents, ISegmentStore segmentStore)
        {
            _monitoringEvents = monitoringEvents;
            _segmentStore = segmentStore;
        }

        public void Execute(TrackPoint position)
        {
            _segments = _segmentStore.LoadSegments();
            
            /*
             * Next steps:
             * - Determine which segment we're on based on the position
             * - Determine direction on that segment (to which end of the segment are we moving)
             * - Determine next turn action (left/straight/right)
             */
            var matchingSegments = _segments
                .Where(s => s.Contains(position))
                .ToList();

            if (!matchingSegments.Any())
            {
                _monitoringEvents.Warning("Could not find a segment for current position {Position}", position.CoordinatesDecimal);
                
                _currentSegment = null;
                _currentDirection = SegmentDirection.Unknown;
                _previousPositionOnSegment = null;
            }
            else
            {
                // This'll get messy when approaching an intersection
                // but we should be able to solve that because we're
                // telling the game where to go and we know which segment
                // that is.
                // So when we set up the turn to make we should also set
                // the target segment somewhere and use that value here.
                var segment = matchingSegments.First();

                if (segment != _currentSegment)
                {
                    if (_currentSegment == null)
                    {
                        _monitoringEvents.Information("Starting in {Segment}", segment.Id);
                    }
                    else
                    {
                        _monitoringEvents.Information("Moved from {CurrentSegment} to {NewSegment}", _currentSegment?.Id, segment.Id);
                    }
                    _currentSegment = segment;

                    // Set for the next position update.
                    // For this we need to use the TrackPoint on the segment instead
                    // of the game position because otherwise we can't determine
                    // direction on the segment later on
                    _previousPositionOnSegment = segment.GetClosestPositionOnSegment(position);

                    // Reset the direction
                    _currentDirection = SegmentDirection.Unknown;

                    // Reset available turns
                    _availableTurnCommands.Clear();
                }
                else
                {
                    // When we have a previous position on this segment
                    // we can determine the direction on the segment.
                    if (_previousPositionOnSegment != null && _currentDirection == SegmentDirection.Unknown)
                    {
                        var currentPositionOnSegment = segment.GetClosestPositionOnSegment(position);
                        var direction = _currentSegment.DirectionOf(_previousPositionOnSegment, currentPositionOnSegment);

                        // If we have a direction then check if we changed
                        // direction on the segment (a U-turn in the game).
                        // If the direction changed we can show which turns
                        // are available.
                        if (direction != SegmentDirection.Unknown && direction != _currentDirection)
                        {
                            _monitoringEvents.Information("Direction is now {Direction}", direction);
                            _currentDirection = direction;
                            
                            var turns = _currentSegment.NextSegments(_currentDirection);
                            
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
                    }

                    // Set for the next position update.
                    // For this we need to use the TrackPoint on the segment instead
                    // of the game position because otherwise we can't determine
                    // direction on the segment later on
                    _previousPositionOnSegment = segment.GetClosestPositionOnSegment(position);
                }
            }
        }
    }
}
