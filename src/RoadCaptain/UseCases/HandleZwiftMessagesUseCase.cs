using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleZwiftMessagesUseCase
    {
        private readonly IMessageEmitter _emitter;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private readonly ISegmentStore _segmentStore;
        private bool _pingedBefore;
        private static readonly object SyncRoot = new();
        private List<Segment> _segments;
        private Segment _currentSegment;
        private TrackPoint _previousPositionOnSegment;
        private SegmentDirection _currentDirection;

        public HandleZwiftMessagesUseCase(
            IMessageEmitter emitter,
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver,
            ISegmentStore segmentStore)
        {
            _emitter = emitter;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
            _segmentStore = segmentStore;
        }
        
        public void Execute(CancellationToken token)
        {
            _segments = _segmentStore.LoadSegments();

            while (!token.IsCancellationRequested)
            {
                // Dequeue will block if there are no messages in the queue
                var message = _emitter.Dequeue(token);

                if (message is ZwiftRiderPositionMessage riderPosition)
                {
                    HandleRiderPositionMessage(riderPosition);
                }
                else if (message is ZwiftPingMessage ping)
                {
                    HandlePingMessage(ping);
                }
                else if (message is ZwiftCommandAvailableMessage commandAvailable)
                {
                    _monitoringEvents.CommandAvailable(commandAvailable.Type);
                }
                else if (message is ZwiftPowerUpMessage powerUp)
                {
                    _monitoringEvents.PowerUpAvailable(powerUp.Type);
                }
            }
        }

        private void HandleRiderPositionMessage(ZwiftRiderPositionMessage riderPosition)
        {
            _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);
            
            /*
             * Next steps:
             * - Determine which segment we're on based on the position
             * - Determine direction on that segment (to which end of the segment are we moving)
             * - Determine next turn action (left/straight/right)
             */

            var position = TrackPoint.FromGameLocation((decimal)riderPosition.Latitude, (decimal)riderPosition.Longitude, (decimal)riderPosition.Altitude);

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
                    _previousPositionOnSegment = null;
                }
                else
                {
                    // When we have a previous position on this segment
                    // we can determine the direction on the segment.
                    if (_previousPositionOnSegment != null)
                    {
                        var direction = _currentSegment.DirectionOf(_previousPositionOnSegment, position);

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
                    else
                    {
                        _monitoringEvents.Information("Did not have previous position in segment");
                    }

                    // Set for the next position update
                    _previousPositionOnSegment = position;
                }
            }
        }

        private void HandlePingMessage(ZwiftPingMessage ping)
        {
            if (!_pingedBefore)
            {
                lock (SyncRoot)
                {
                    if (_pingedBefore)
                    {
                        return;
                    }

                    _pingedBefore = true;
                }

                _messageReceiver.SendInitialPairingMessage(ping.RiderId);
            }
        }
    }
}