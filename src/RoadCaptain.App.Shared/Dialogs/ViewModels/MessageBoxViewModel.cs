// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class MessageBoxViewModel : ViewModelBase
    {
        private readonly MessageBoxButton _buttonOptions;
        private readonly MessageBoxIcon _icon;

        public MessageBoxViewModel(MessageBoxButton buttonOptions, string title, string message,
            MessageBoxIcon icon)
        {
            Message = message;
            _buttonOptions = buttonOptions;
            _icon = icon;
            Title = title;
        }

        public string Title { get; }
        public string Message { get; }

        public bool ShowOkButton => _buttonOptions is MessageBoxButton.Ok or MessageBoxButton.OkCancel;

        public bool ShowYesButton =>
            _buttonOptions is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel;

        public bool ShowNoButton =>
            _buttonOptions is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel;

        public bool ShowCancelButton => _buttonOptions is MessageBoxButton.YesNoCancel or MessageBoxButton.RetryCancel or MessageBoxButton.CancelTryContinue;
        public bool ShowAbortButton => _buttonOptions == MessageBoxButton.AbortRetryIgnore;
        public bool ShowRetryButton => _buttonOptions is MessageBoxButton.AbortRetryIgnore or MessageBoxButton.RetryCancel;
        public bool ShowTryAgainButton => _buttonOptions == MessageBoxButton.CancelTryContinue;
        public bool ShowContinueButton => _buttonOptions == MessageBoxButton.CancelTryContinue;
        public bool ShowIgnoreButton => _buttonOptions == MessageBoxButton.AbortRetryIgnore;

        public Bitmap Icon
        {
            get
            {
                switch (_icon)
                {
                    case MessageBoxIcon.Error:
                        return FromResource("error");
                    case MessageBoxIcon.Information:
                    case MessageBoxIcon.Question:
                        return FromResource("question");
                    case MessageBoxIcon.Warning:
                        return FromResource("warning");
                    default:
                        return null;
                }
            }
        }

        private Bitmap FromResource(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var uri = new Uri($"avares://RoadCaptain.App.Shared/Assets/{name}.png");
            var asset = assets.Open(uri);

            return new Bitmap(asset);
        }
    }
}
