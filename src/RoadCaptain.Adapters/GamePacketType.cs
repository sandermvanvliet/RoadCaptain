// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Adapters
{
    internal enum GamePacketType
    {
        SPORTS_DATA_REQUEST = 1,
        SPORTS_DATA_RESPONSE = 2,
        GAME_SESSION_INFO = 3,
        CLIENT_INFO = 4,
        MAPPING_DATA = 5,
        INTERSECTION_AHEAD = 6,
        PLAYER_INFO = 7,
        RIDE_ON_BOMB_REQUEST = 8,
        RIDE_ON_BOMB_RESPONSE = 9,
        EFFECT_REQUEST = 10,
        WORKOUT_INFO = 11,
        WORKOUT_STATE = 12,
        PLAYER_FITNESS_INFO = 13,
        WORKOUT_ACTION_REQUEST = 14,
        CLIENT_ACTION = 15,
        MEETUP_STATE = 16,
        SEGMENT_RESULT_ADD = 17,
        SEGMENT_RESULT_REMOVE = 18,
        SEGMENT_RESULT_NEW_LEADER = 19,
        PLAYER_ACTIVE_SEGMENTS = 20,
        PLAYER_STOPWATCH_SEGMENT = 21,
        BOOST_MODE_STATE = 22,
        GAME_ACTION = 23,
        USER_ACTION_SET = 24,
        USER_ACTION_ACTION = 25
    }
}
