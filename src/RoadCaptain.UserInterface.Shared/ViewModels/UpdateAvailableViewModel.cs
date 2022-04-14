using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RoadCaptain.UserInterface.Shared.Annotations;
using RoadCaptain.UserInterface.Shared.Commands;

namespace RoadCaptain.UserInterface.Shared.ViewModels
{
    public class UpdateAvailableViewModel : INotifyPropertyChanged
    {
        private string _version;
        private string _downloadLink;
        private string _releaseNotes;

        public UpdateAvailableViewModel(Release release)
        {
            Version = release.Version.ToString(4);
            DownloadLink = release.InstallerDownloadUri?.ToString();
            ReleaseNotes = release.ReleaseNotes;

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string),
                _ => true);
        }

        public ICommand OpenLinkCommand { get; }

        public string DownloadLink
        {
            get => _downloadLink;
            set
            {
                if (value == _downloadLink) return;
                _downloadLink = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            set
            {
                if (value == _releaseNotes) return;
                _releaseNotes = value;
                OnPropertyChanged();
            }
        }

        private CommandResult OpenLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var startInfo = new ProcessStartInfo(uri.ToString())
                {
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                return CommandResult.Success();
            }

            return CommandResult.Failure("Invalid url");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}