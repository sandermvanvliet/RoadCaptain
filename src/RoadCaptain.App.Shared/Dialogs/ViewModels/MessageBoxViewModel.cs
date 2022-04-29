namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class MessageBoxViewModel : ViewModelBase
    {
        private readonly MessageBoxButton _buttonOptions;

        public MessageBoxViewModel(MessageBoxButton buttonOptions, string title, string message)
        {
            Message = message;
            _buttonOptions = buttonOptions;
            Title = title;
        }

        public string Title { get; }
        public string Message { get; }

        public bool ShowOkButton => _buttonOptions == MessageBoxButton.Ok;

        public bool ShowYesButton =>
            _buttonOptions == MessageBoxButton.YesNo || _buttonOptions == MessageBoxButton.YesNoCancel;

        public bool ShowNoButton =>
            _buttonOptions == MessageBoxButton.YesNo || _buttonOptions == MessageBoxButton.YesNoCancel;

        public bool ShowCancelButton => _buttonOptions == MessageBoxButton.YesNoCancel;
        public bool ShowCloseButton => _buttonOptions == MessageBoxButton.Close;
    }
}