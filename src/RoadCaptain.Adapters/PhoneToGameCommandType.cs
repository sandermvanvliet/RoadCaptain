// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Adapters
{
    internal enum PhoneToGameCommandType
    {
        PhoneToGameUnknownCommand = 0,
        ChangeCameraAngle = 1,
        JoinAnotherPlayer = 2,
        TeleportToStart = 3,
        ElbowFlick = 4,
        Wave = 5,
        RideOn = 6,
        Bell = 7,
        HammerTime = 8,
        Toast = 9,
        Nice = 10,
        BringIt = 11,
        DoneRiding = 14,
        CancelDoneRiding = 15,
        DiscardActivity = 12,
        SaveActivity = 13,
        RequestForProfile = 16,
        TakeScreenshot = 17,
        ObsoleteGroupTextMessage = 18,
        ObsoleteSinglePlayerTextMessage = 19,
        MobileApiVersion = 20,
        ActivatePowerUp = 21,
        CustomAction = 22,
        UTurn = 23,
        FanView = 24,
        SocialPlayerAction = 25,
        MobileAlertResponse = 26,
        BleperipheralResponse = 27,
        PairingAs = 28,
        PhoneToGamePacket = 29,
        BleperipheralDiscovery = 30
    }
}
