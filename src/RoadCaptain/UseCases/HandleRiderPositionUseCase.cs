using System;
using System.Collections.Generic;
using System.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleRiderPositionUseCase
    {
        private List<Segment> _segments;
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
                
                _dispatcher.SegmentChanged(null);

                _currentDirection = SegmentDirection.Unknown;
                _dispatcher.DirectionChanged(_currentDirection);
                _previousPositionOnSegment = null;
            }
            else
            {
                var (segment, closestOnSegment) = GetClosestMatchingSegment(matchingSegments, position);

                if (segment != _dispatcher.CurrentSegment)
                {
                    _dispatcher.SegmentChanged(segment);

                    // Set for the next position update.
                    // For this we need to use the TrackPoint on the segment instead
                    // of the game position because otherwise we can't determine
                    // direction on the segment later on
                    _previousPositionOnSegment = closestOnSegment;

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
                        var currentPositionOnSegment = closestOnSegment;
                        var direction = _dispatcher.CurrentSegment.DirectionOf(_previousPositionOnSegment, currentPositionOnSegment);

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
                    _previousPositionOnSegment = closestOnSegment;
                }
            }
        }

        private static (Segment,TrackPoint) GetClosestMatchingSegment(List<Segment> segments, TrackPoint position)
        {
            // If there is only one segment then we can
            // do a quick exist and not bother to figure
            // out which point is actually closest.
            if (segments.Count == 1)
            {
                return (segments[0], segments[0].GetClosestPositionOnSegment(position));
            }

            // For each segment find the closest track point in that segment
            // in relation to the current position
            TrackPoint closest = null;
            decimal? closestDistance = null;
            Segment closestSegment = null;

            foreach (var segment in segments)
            {
                // This is very suboptimal as this needs to traverse
                // all the points of the segment whereas finding if
                // the point is on the segment can stop at the first
                // hit.
                var closestOnSegment = segment
                    .Points
                    .Select(p => new { Point = p, Distance = p.DistanceTo(position) })
                    .OrderBy(d => d.Distance)
                    .First();

                if (closest == null)
                {
                    closest = closestOnSegment.Point;
                    closestDistance = closestOnSegment.Distance;
                    closestSegment = segment;
                }
                else if (closestOnSegment.Distance < closestDistance)
                {
                    closest = closestOnSegment.Point;
                    closestDistance = closestOnSegment.Distance;
                    closestSegment = segment;
                }
            }

            return (closestSegment, closest);
        }
    }
}
