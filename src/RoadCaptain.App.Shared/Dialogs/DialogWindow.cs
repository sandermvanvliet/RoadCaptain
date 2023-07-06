// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia.Controls;

namespace RoadCaptain.App.Shared.Dialogs
{
    public abstract class DialogWindow : Window
    {
        public DialogResult DialogResult { get; protected set; } = DialogResult.Unknown;
    }
}
