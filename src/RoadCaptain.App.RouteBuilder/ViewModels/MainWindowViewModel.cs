// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.Services;
using CommandResult = RoadCaptain.App.Shared.Commands.CommandResult;
using RelayCommand = RoadCaptain.App.Shared.Commands.RelayCommand;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string? _version;
        private string? _changelogUri;
        private bool _haveCheckedVersion;
        private readonly IVersionChecker _versionChecker;
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;
        private bool _haveCheckedLastOpenedVersion;
        private readonly IApplicationFeatures _applicationFeatures;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker,
            IWindowService windowService, IWorldStore worldStore, IUserPreferences userPreferences,
            IApplicationFeatures applicationFeatures, IStatusBarService statusBarService)
        {
            _versionChecker = versionChecker;
            _windowService = windowService;
            _userPreferences = userPreferences;
            _applicationFeatures = applicationFeatures;

            Model = new MainWindowModel();
            Route = new RouteViewModel(routeStore, segmentStore);
            LandingPageViewModel = new LandingPageViewModel(worldStore, userPreferences, windowService);
            LandingPageViewModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(LandingPageViewModel.SelectedWorld):
                    case nameof(LandingPageViewModel.SelectedSport):
                        if (LandingPageViewModel is { SelectedSport: not null, SelectedWorld: not null })
                        {
                            Route.World = worldStore.LoadWorldById(LandingPageViewModel.SelectedWorld.Id);
                            Route.Sport = LandingPageViewModel.SelectedSport.Sport;
                        }
                        break;
                }
            };
            
            BuildRouteViewModel = new BuildRouteViewModel(Route, userPreferences, windowService, worldStore, segmentStore, statusBarService);

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                param => !string.IsNullOrEmpty(param as string));

            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";
            
            statusBarService.Subscribe(message => Model.StatusBarInfo(message), message => Model.StatusBarWarning(message), message => Model.StatusBarError(message));
        }

        public MainWindowModel Model { get; }
        public RouteViewModel Route { get; }

        public ICommand OpenLinkCommand { get; set; }

        public LandingPageViewModel LandingPageViewModel { get; }

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

        public string Version
        {
            get => _version ?? "0.0.0.0";
            set
            {
                if (value == _version) return;
                _version = value;
                ChangelogUri =
                    $"https://github.com/sandermvanvliet/RoadCaptain/blob/main/Changelog.md/#{Version.Replace(".", "")}";
                this.RaisePropertyChanged();
            }
        }

        public string? ChangelogUri
        {
            get => _changelogUri;
            set
            {
                if (value == _changelogUri) return;
                _changelogUri = value;
                this.RaisePropertyChanged();
            }
        }

        public BuildRouteViewModel BuildRouteViewModel { get; }

        public async Task CheckForNewVersion()
        {
            if (_haveCheckedVersion)
            {
                return;
            }

            _haveCheckedVersion = true;

            var currentVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            if (latestRelease.official != null && latestRelease.official.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease.official);
            }
            else if (_applicationFeatures.IsPreRelease && 
                     latestRelease.preRelease != null &&
                     latestRelease.preRelease.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease.preRelease);
            }
        }

        public async Task CheckLastOpenedVersion()
        {
            if (_haveCheckedLastOpenedVersion)
            {
                return;
            }

            _haveCheckedLastOpenedVersion = true;

            var previousVersion = _userPreferences.LastOpenedVersion;
            var thisVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            // If there is a newer version available don't display anything
            if (latestRelease.official != null && latestRelease.official.Version > thisVersion)
            {
                return;
            }

            if (_applicationFeatures.IsPreRelease &&
                latestRelease.preRelease != null &&
                latestRelease.preRelease.Version > thisVersion)
            {
                return;
            }

            // If this version is newer than the previous one shown and it's
            // the latest version then show the what is new dialog
            if (latestRelease.official != null && thisVersion > previousVersion && thisVersion == latestRelease.official.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease.official);
            }
            else if (latestRelease.preRelease != null && thisVersion > previousVersion && thisVersion == latestRelease.preRelease.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease.preRelease);
            }
        }
    }
}