namespace RoadCaptain.Adapters.Protobuf
{
    public enum CommandType
    {
        Unknown = 0,
        ElbowFlick = 4,
        Wave = 5,
        RideOn = 6,
        SomethingEmpty = 23, // I suspect this is a  "reset" type thing
        TurnLeft = 1010,
        GoStraight = 1011,
        TurnRight = 1012,
        DiscardAero = 1030,
        DiscardLightweight = 1034,
        PowerGraph = 1060,
        HeadsUpDisplay = 1081,
    }
}