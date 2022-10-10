// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class AboutRoadCaptainViewModel : ViewModelBase
    {
        private string _version;

        public AboutRoadCaptainViewModel()
        {
            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink("https://github.com/sandermvanvliet/RoadCaptain/"),
                _ => true);
        }

        public ICommand OpenLinkCommand { get; }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                this.RaisePropertyChanged();
            }
        }

        private CommandResult OpenLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
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
