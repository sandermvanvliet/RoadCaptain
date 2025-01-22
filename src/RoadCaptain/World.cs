// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Drawing;

namespace RoadCaptain
{
    public class World
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public SpawnPoint[]? SpawnPoints { get; set; }
        public WorldStatus Status { get; set; }
        public ZwiftWorldId ZwiftId { get; set; } = ZwiftWorldId.Unknown;
        /// <summary>
        /// The latitude/longitude of the most left point in this world which corresponds to the <see cref="MapMostLeft"/> x, y position on the map image
        /// </summary>
        public TrackPoint? WorldMostLeft { get; set; }
        /// <summary>
        /// The latitude/longitude of the most right point in this world which corresponds to the <see cref="MapMostRight"/> x, y position on the map image
        /// </summary>
        public TrackPoint? WorldMostRight { get; set; }
        public PointF? MapMostLeft { get; set; }
        public PointF? MapMostRight { get; set; }
    }

    public enum WorldStatus
    {
        Unknown,
        Available,
        Unavailable,
        Beta
    }

    public enum ZwiftWorldId
    {
        Unknown = -1,
        Watopia = 1,
        Richmond = 2,
        London = 3,
        NewYork = 4,
        Innsbruck = 5,
        Bologna = 6,
        Yorkshire = 7,
        CritCity = 8,
        MakuriIslands = 9,
        France = 10,
        Paris = 11
    }
}
