// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class UpdateAvailableViewModel : ViewModelBase
    {
        private string _version;
        private string _downloadLink;
        private string _releaseNotes;
        private string _title;

        public UpdateAvailableViewModel(Release release)
        {
            _version = release.Version.ToString(4);
            _downloadLink = release.InstallerDownloadUri?.ToString() ?? "https://github.com/sandermvanvliet/RoadCaptain/";
            _releaseNotes = release.ReleaseNotes;
            _title = release.IsPreRelease
                ? "RoadCaptain test release available"
                : "RoadCaptain update available";

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => true);
        }

        public ICommand OpenLinkCommand { get; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                this.RaisePropertyChanged();
            }
        }
        public string DownloadLink
        {
            get => _downloadLink;
            set
            {
                if (value == _downloadLink) return;
                _downloadLink = value;
                this.RaisePropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                this.RaisePropertyChanged();
            }
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            set
            {
                if (value == _releaseNotes) return;
                _releaseNotes = value;
                this.RaisePropertyChanged();
            }
        }

        private CommandResult OpenLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                // Code from Avalonia: AboutAvaloniaDialog.cs
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? url : "",
                    CreateNoWindow = true,
                    UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                });

                return CommandResult.Success();
            }

            return CommandResult.Failure("Invalid url");
        }
    }
}
