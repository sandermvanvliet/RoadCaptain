// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.Commands;
using CommandResult = RoadCaptain.App.Shared.Commands.CommandResult;
using RelayCommand = RoadCaptain.App.Shared.Commands.RelayCommand;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

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
        private readonly SearchRoutesUseCase _searchRoutesUseCase;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore, IVersionChecker versionChecker,
            IWindowService windowService, IWorldStore worldStore, IUserPreferences userPreferences,
            IApplicationFeatures applicationFeatures, IStatusBarService statusBarService, SearchRoutesUseCase searchRoutesUseCase)
        {
            _versionChecker = versionChecker;
            _windowService = windowService;
            _userPreferences = userPreferences;
            _applicationFeatures = applicationFeatures;
            _searchRoutesUseCase = searchRoutesUseCase;

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
                    case nameof(LandingPageViewModel.SelectedRoute):
                        var plannedRoute = LandingPageViewModel.SelectedRoute?.AsRouteModel()?.PlannedRoute;
                        if (plannedRoute != null)
                        {
                            Route.LoadFromPlannedRoute(plannedRoute);
                        }
                        break;
                }
            };
            
            BuildRouteViewModel = new BuildRouteViewModel(Route, userPreferences, windowService, worldStore, segmentStore, statusBarService);

            OpenLinkCommand = new RelayCommand(
                link => OpenLink(link as string ?? throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
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

        public async Task LoadMyRoutes()
        {
            var currentUser = "Sander van Vliet [RoadCaptain]";
            var result = await _searchRoutesUseCase.ExecuteAsync(new SearchRouteCommand(RetrieveRepositoriesIntent.Manage, creator: currentUser));

            LandingPageViewModel.MyRoutes = result
                .Select(r => new Shared.ViewModels.RouteViewModel(r))
                .ToArray();
        }
    }
}