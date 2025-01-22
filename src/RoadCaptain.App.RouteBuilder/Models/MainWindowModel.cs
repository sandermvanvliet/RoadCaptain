// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using JetBrains.Annotations;

namespace RoadCaptain.App.RouteBuilder.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string _windowTitle = "RoadCaptain - Route Builder";
        private string? _statusBarText;
        private IBrush _statusBarBackground = Brushes.DodgerBlue;
        private IBrush _statusBarForeground = Brushes.White;
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (value == _windowTitle) return;
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public string? StatusBarText
        {
            get => _statusBarText;
            private set
            {
                if (value == _statusBarText) return;
                _statusBarText = value;
                OnPropertyChanged();
            }
        }

        public IBrush StatusBarBackground
        {
            get => _statusBarBackground;
            private set
            {
                if (Equals(value, _statusBarBackground)) return;
                _statusBarBackground = value;
                OnPropertyChanged();
            }
        }

        public IBrush StatusBarForeground
        {
            get => _statusBarForeground;
            private set
            {
                if (Equals(value, _statusBarForeground)) return;
                _statusBarForeground = value;
                OnPropertyChanged();
            }
        }

        public void StatusBarInfo(string format, params object[] args)
        {
            StatusBarText = string.Format(format, args);
            StatusBarBackground = Brushes.DodgerBlue;
            StatusBarForeground = Brushes.White;
        }

        public void StatusBarWarning(string format, params object[] args)
        {
            StatusBarText = string.Format(format, args);
            StatusBarBackground = Brushes.DarkOrange;
            StatusBarForeground = Brushes.White;
        }

        public void StatusBarError(string format, params object[] args)
        {
            StatusBarText = string.Format(format, args);
            StatusBarBackground = Brushes.Red;
            StatusBarForeground = Brushes.White;
        }

        public void ClearStatusBar()
        {
            StatusBarInfo(string.Empty);
        }
    }
}
