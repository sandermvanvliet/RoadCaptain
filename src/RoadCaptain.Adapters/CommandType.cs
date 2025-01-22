// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Adapters
{
    internal enum CommandType
    {
        Unknown = 0,
        ElbowFlick = 4,
        Wave = 5,
        RideOn = 6,
        SomethingEmpty = 23, // This is the synchronisation event for command sequence numbers that are sent to back to the game
        TurnLeft = 1010,
        GoStraight = 1011,
        TurnRight = 1012,
        DiscardAero = 1030,
        DiscardLightweight = 1034,
        PowerGraph = 1060,
        HeadsUpDisplay = 1081,
    }
}
