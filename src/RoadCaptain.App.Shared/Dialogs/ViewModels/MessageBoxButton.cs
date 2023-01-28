// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
