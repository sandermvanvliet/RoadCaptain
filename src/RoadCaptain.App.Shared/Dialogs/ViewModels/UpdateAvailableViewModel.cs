﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class UpdateAvailableViewModel : ViewModelBase
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
    }
}