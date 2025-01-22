// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain.Tests.Unit.Coordinates
{
    internal class MapInfo
    {
        public const int WATOPIA = 1;
        public const int RICHMOND = 2;
        public const int LONDON = 3;
        public const int NEWYORK = 4;
        public const int INNSBRUCK = 5;
        public const int BOLOGNA = 6;
        public const int YORKSHIRE = 7;
        public const int CRITCITY = 8;
        public const int MAKURI = 9;
        public const int FRANCE = 10;
        public const int PARIS = 11;
        public const int GRAVELMTN = 12;

        public static readonly Dictionary<int, float[]> LATITUDE_LONGITUDE_OFFSETS = new()
        {
            { WATOPIA, new[] { -11.644904f, 166.95293f } },
            { RICHMOND, new[] { 37.543f, -77.4374f } },
            { LONDON, new[] { 51.501705f, -0.16794094f } },
            { NEWYORK, new[] { 40.76723f, -73.97667f } },
            { INNSBRUCK, new[] { 47.2728f, 11.39574f } },
            { BOLOGNA, new[] { 44.49477f, 11.34324f } },
            { YORKSHIRE, new[] { 53.991127f, -1.541751f } },
            { CRITCITY, new[] { -10.3844f, 165.8011f } },
            { MAKURI, new[] { -10.749806f, 165.83644f } },
            { FRANCE, new[] { -21.695074f, 166.19745f } },
            { PARIS, new[] { 48.86763f, 2.31413f } },
            { GRAVELMTN, new[] { -10.38469f, 165.8222f } },
        };

        public static readonly Dictionary<int, float[]> LATITUDE_LONGITUDE_DEGREE_DISTANCE = new()
        {
            { WATOPIA, new[] { 110614.71f, 109287.52f } },
            { RICHMOND, new[] { 110987.82f, 88374.68f } },
            { LONDON, new[] { 111258.3f, 69400.28f } },
            { NEWYORK, new[] { 110850.0f, 84471.0f } },
            { INNSBRUCK, new[] { 111230.0f, 75027.0f } },
            { BOLOGNA, new[] { 111230.0f, 79341.0f } },
            { YORKSHIRE, new[] { 111230.0f, 65393.0f } },
            { CRITCITY, new[] { 110614.71f, 109287.52f } },
            { MAKURI, new[] { 110614.71f, 109287.52f } },
            { FRANCE, new[] { 110726.0f, 103481.0f } },
            { PARIS, new[] { 111230.0f, 73167.0f } },
            { GRAVELMTN, new[] { 111230.0f, 109408.0f } },
        };

        public static readonly RectF WORLD_WATOPIA = new RectF(-824500.0f, -209700.0f, 869900.0f, 637500.0f);
        public static readonly RectF WORLD_LONDON = new RectF(-67200.0f, -383900.0f, 780000.0f, 463300.0f);
        public static readonly RectF WORLD_RICHMOND = new RectF(-462300.0f, -383600.0f, 384900.0f, 463600.0f);
        public static readonly RectF WORLD_NEWYORK = new RectF(-387400.0f, -554700.0f, 459800.0f, 292500.0f);
        public static readonly RectF WORLD_INNSBRUCK = new RectF(-342800.0f, -243700.0f, 649700.0f, 748800.0f);
        public static readonly RectF WORLD_BOLOGNA = new RectF(-637400.0f, -400700.0f, 209800.0f, 446500.0f);
        public static readonly RectF WORLD_YORKSHIRE = new RectF(-589365.0f, -380288.0f, 257835.0f, 466912.0f);
        public static readonly RectF WORLD_CRITCITY = new RectF(-207100.0f, -208000.0f, 216500.0f, 215600.0f);
        public static readonly RectF WORLD_MAKURI = new RectF(-710600.0f, -346900.0f, 136600.0f, 500300.0f);
        public static readonly RectF WORLD_FRANCE = new RectF(-905139.1f, -814379.2f, 789860.9f, 880620.8f);
        public static readonly RectF WORLD_PARIS = new RectF(-211800.0f, -211900.0f, 211800.0f, 211700.0f);
        public static readonly RectF WORLD_GRAVELMTN = new RectF(-107400.0f, -314000.0f, 103100.0f, -103500.0f);
    }

    internal struct RectF
    {
        public float Left { get; }
        public float Top { get; }
        public float Right { get; }
        public float Bottom { get; }

        public RectF(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Width => Right - Left;
        public float Height => Bottom - Top;
    }
}
