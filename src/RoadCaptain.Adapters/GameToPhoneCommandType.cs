namespace RoadCaptain.Adapters
{
    internal enum GameToPhoneCommandType
    {
        UnknownCommand = 0,
        ClearPowerUp = 1,
        SetPowerUp = 2,
        ActivatePowerUp = 3,
        CustomizeActionButton = 4,
        SendImage = 5,
        SocialPlayerAction = 6,
        DontUseMobileAlert = 7,
        BleperipheralRequest = 8,
        PairingStatus = 9,
        MobileAlertCancel = 10,
        DefaultActivityName = 11,
        MobileAlert = 12,
        Packet = 13,
    }
}