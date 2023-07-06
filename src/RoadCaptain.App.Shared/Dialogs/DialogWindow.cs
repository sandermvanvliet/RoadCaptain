using Avalonia.Controls;

namespace RoadCaptain.App.Shared.Dialogs
{
    public abstract class DialogWindow : Window
    {
        public DialogResult DialogResult { get; protected set; } = DialogResult.Unknown;
    }
}