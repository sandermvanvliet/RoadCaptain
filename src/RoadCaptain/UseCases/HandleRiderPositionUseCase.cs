using System;
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
        private readonly IGameStateDispatcher _dispatcher;
        private TrackPoint _previousPositionInGame;

        public HandleRiderPositionUseCase(MonitoringEvents monitoringEvents, ISegmentStore segmentStore, IGameStateDispatcher dispatcher)
        {
            _monitoringEvents = monitoringEvents;
            _segmentStore = segmentStore;
            _dispatcher = dispatcher;
        }

        public void Execute(TrackPoint position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            // Debounce position updates.
            // Especially when the rider is not yet moving this usually
            // stays at the same location for a long time (world origin mostly)
            if (position.Equals(_previousPositionInGame))
            {
                return;
            }

            // Store previous position so we can debounce on the next update.
            _previousPositionInGame = position;

            // Update game state
            _dispatcher.PositionChanged(position);
            
            // We don't want to load this in the constructor as that
            // may happen way too soon before initialisation is fully
            // complete and that leads to weird errors.
            _segments ??= _segmentStore.LoadSegments();
            
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
                _dispatcher.SegmentChanged(_currentSegment);

                _currentDirection = SegmentDirection.Unknown;
                _dispatcher.DirectionChanged(_currentDirection);
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
                    _currentSegment = segment;
                    _dispatcher.SegmentChanged(_currentSegment);

                    // Set for the next position update.
                    // For this we need to use the TrackPoint on the segment instead
                    // of the game position because otherwise we can't determine
                    // direction on the segment later on
                    _previousPositionOnSegment = segment.GetClosestPositionOnSegment(position);

                    // TODO: Determine this from the segment we came from
                    // as we have a turn in the opposite direction of the
                    // current segment
                    _currentDirection = SegmentDirection.Unknown;
                    _dispatcher.DirectionChanged(_currentDirection);
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
                            _currentDirection = direction;
                            _dispatcher.DirectionChanged(_currentDirection);
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
