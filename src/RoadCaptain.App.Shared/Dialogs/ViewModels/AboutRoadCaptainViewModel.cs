using System;
using System.Diagnostics;
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