namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public enum MessageBoxButton
    {
        Ok = 0,
        OkCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
        CancelTryContinue = 6,
    }

    public enum MessageBoxIcon
    {
        None,
        Question,
        Information,
        Error,
        Warning
    }
}