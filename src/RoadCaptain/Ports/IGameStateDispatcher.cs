﻿using System.Collections.Generic;

namespace RoadCaptain.Ports
{
    /*
     * Use this either as a singleton in-memory approach or implement this on both the sending and consuming sides of a queue
     */
    public interface IGameStateDispatcher
    {
        /// <summary>
        /// The position of the rider in the game has changed.
        /// </summary>
        /// <remarks>Only fired when the position is different from the last</remarks>
        /// <param name="position"></param>
        void PositionChanged(TrackPoint position);

        /// <summary>
        /// The segment that the rider is on has changed
        /// </summary>
        /// <param name="segment"></param>
        void SegmentChanged(Segment segment);

        /// <summary>
        /// The rider is coming up to the end of a segment and there are options to turn onto another segment
        /// </summary>
        /// <param name="turns">The available turns (direction + next segment)</param>
        /// <remarks>If the current segment is a direct connection to only one segment this is not called</remarks>
        void TurnsAvailable(List<Turn> turns);

        /// <summary>
        /// The direction of the rider on the segment has changed
        /// </summary>
        /// <param name="direction">The direction the rider is heading in</param>
        /// <remarks>This happens when a u-turn is executed. Otherwise it should not occcur much</remarks>
        void DirectionChanged(SegmentDirection direction);
    }
}