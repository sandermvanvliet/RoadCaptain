using System;
using System.Collections.Generic;
using System.Threading;

namespace RoadCaptain.Ports
{
    public interface IGameStateReceiver
    {
        void Start(CancellationToken token);
        void Register(
            Action<TrackPoint> positionChanged,
            Action<string> segmentChanged,
            Action<List<Turn>> turnsAvailable,
            Action<SegmentDirection> directionChanged,
            Action<List<TurnDirection>> turnCommandsAvailable,
            Action<ulong> enteredGame,
            Action<ulong> leftGame);
    }
}