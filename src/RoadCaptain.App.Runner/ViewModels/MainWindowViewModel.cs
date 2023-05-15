// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.IdentityModel.JsonWebTokens;
using ReactiveUI;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUserPreferences _userPreferences;
        private readonly Configuration _configuration;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private readonly IWindowService _windowService;
        private bool _loggedInToZwift;
        private string? _routePath;
        private string _windowTitle = "RoadCaptain";
        private string? _zwiftAvatarUri = "avares://RoadCaptain.App.Shared/Assets/profile-default.png";
        private string? _zwiftName;
        private readonly IRouteStore _routeStore;
        private string _version = "0.0.0.0";
        private string? _changelogUri;
        private RouteModel? _route = new();
        private readonly IVersionChecker _versionChecker;
        private readonly ISegmentStore _segmentStore;
        private bool _haveCheckedVersion;
        private IImage? _zwiftAvatar;
        private bool _endActivityAtEndOfRoute;
        private readonly IZwiftCredentialCache _credentialCache;
        private bool _haveCheckedLastOpenedVersion;
        private readonly IApplicationFeatures _applicationFeatures;

        public MainWindowViewModel(Configuration configuration,
            IUserPreferences userPreferences,
            IWindowService windowService,
            IGameStateDispatcher gameStateDispatcher,
            IRouteStore routeStore,
            IVersionChecker versionChecker,
            ISegmentStore segmentStore,
            IZwiftCredentialCache credentialCache, 
            MonitoringEvents monitoringEvents, 
            IApplicationFeatures applicationFeatures)
        {
            _configuration = configuration;
            _userPreferences = userPreferences;
            _windowService = windowService;
            _gameStateDispatcher = gameStateDispatcher;
            _routeStore = routeStore;
            _versionChecker = versionChecker;
            _segmentStore = segmentStore;
            _credentialCache = credentialCache;
            _applicationFeatures = applicationFeatures;


            try
            {
                if (!string.IsNullOrEmpty(configuration.Route))
                {
                    LoadRouteFromPath(configuration.Route);
                }
                else if (!string.IsNullOrEmpty(userPreferences.Route))
                {
                    LoadRouteFromPath(userPreferences.Route);
                }
            }
            catch (MissingSegmentException ex)
            {
                monitoringEvents.Error(ex, "Failed to load route");
            }

            StartRouteCommand = new RelayCommand(
                    _ => StartRoute(),
                    _ => CanStartRoute
                )
                .SubscribeTo(this, () => CanStartRoute);

            LoadRouteCommand = new AsyncRelayCommand(
                _ => LoadRoute(),
                _ => true)
                .OnFailure(async result =>
                {
                    await _windowService.ShowErrorDialog(result.Message);
                    // Clear the current route
                    Route = new RouteModel();
                });

            LogInCommand = new AsyncRelayCommand(
                    _ => LogInToZwift(_ as Window ?? throw new ArgumentNullException(nameof(AsyncRelayCommand.CommandParameter))),
                    _ => !LoggedInToZwift)
                .SubscribeTo(this, () => LoggedInToZwift);

            BuildRouteCommand = new AsyncRelayCommand(
                    _ => LaunchRouteBuilder(),
                    _ => true)
                .OnFailure(async result => await _windowService.ShowErrorDialog(result.Message));

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string ?? throw new ArgumentNullException(nameof(AsyncRelayCommand.CommandParameter))),
                _ => !string.IsNullOrEmpty(_ as string));

            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";

            RebelRoutes = LoadRebelRoutes();
        }

        private void LoadRouteFromPath(string? routePath)
        {
            if (string.IsNullOrEmpty(routePath))
            {
                return;
            }

            try
            {
                var plannedRoute = _routeStore.LoadFrom(routePath);

                RoutePath = routePath;

                _userPreferences.LastUsedFolder = Path.GetDirectoryName(RoutePath);
                _userPreferences.Save();

                var routeFileName = RoutePath;

                try
                {
                    routeFileName = Path.GetFileName(routeFileName);
                }
                catch
                {
                    /* nop */
                }

                WindowTitle = $"RoadCaptain - {routeFileName}";

                Route = RouteModel.From(
                    plannedRoute,
                    _segmentStore.LoadSegments(
                        plannedRoute.World,
                        plannedRoute.Sport),
                    _segmentStore.LoadMarkers(plannedRoute.World));

                _gameStateDispatcher.RouteSelected(Route!.PlannedRoute);
            }
            catch (FileNotFoundException)
            {
            }
            catch (InvalidOperationException)
            {
                // Route created with newer version or something similar
            }
        }

        private List<PlannedRoute> LoadRebelRoutes()
        {
            return Directory
                .GetFiles(
                    Path.Combine(Environment.CurrentDirectory, "Routes"),
                    "RebelRoute-*.json")
                .Select(file => _routeStore.LoadFrom(file))
                .ToList();
        }

        public bool CanStartRoute =>
            Route?.PlannedRoute != null &&
            LoggedInToZwift;

        public string? RoutePath
        {
            get => _routePath;
            set
            {
                if (value == _routePath)
                {
                    return;
                }

                _routePath = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanStartRoute));
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (value == _windowTitle)
                {
                    return;
                }

                _windowTitle = value;
                this.RaisePropertyChanged();
            }
        }

        public bool LoggedInToZwift
        {
            get => _loggedInToZwift;
            set
            {
                if (value == _loggedInToZwift)
                {
                    return;
                }

                _loggedInToZwift = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ZwiftLoggedInText));
                this.RaisePropertyChanged(nameof(CanStartRoute));
            }
        }

        public string ZwiftLoggedInText =>
            LoggedInToZwift
                ? "Logged in to Zwift"
                : "Not yet logged in to Zwift";

        public string? ZwiftAccessToken { get; set; }

        public string? ZwiftName
        {
            get => _zwiftName;
            set
            {
                if (value == _zwiftName)
                {
                    return;
                }

                _zwiftName = value;
                this.RaisePropertyChanged();
            }
        }

        public IImage? ZwiftAvatar
        {
            get => _zwiftAvatar;
            set
            {
                if (value == _zwiftAvatar)
                {
                    return;
                }

                _zwiftAvatar = value;
                this.RaisePropertyChanged();
            }
        }

        public string? ZwiftAvatarUri
        {
            get => _zwiftAvatarUri;
            set
            {
                if (value == _zwiftAvatarUri)
                {
                    return;
                }

                _zwiftAvatarUri = value;
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
                ChangelogUri = $"https://github.com/sandermvanvliet/RoadCaptain/blob/main/Changelog.md/#{Version.Replace(".", "")}";
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

        public RouteModel? Route
        {
            get => _route;
            set
            {
                if (Equals(value, _route)) return;

                _route = value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanStartRoute));
            }
        }

        public bool EndActivityAtEndOfRoute
        {
            get => _endActivityAtEndOfRoute;
            set
            {
                if (value == _endActivityAtEndOfRoute) return;
                _endActivityAtEndOfRoute = value;
                _userPreferences.EndActivityAtEndOfRoute = value;
                this.RaisePropertyChanged();
            }
        }

        public List<PlannedRoute> RebelRoutes
        {
            get;
        }

        public ICommand StartRouteCommand { get; set; }
        public ICommand LoadRouteCommand { get; set; }
        public ICommand LogInCommand { get; set; }
        public ICommand BuildRouteCommand { get; set; }
        public ICommand OpenLinkCommand { get; set; }

        public void UpdateGameState(object state)
        {
            if (state is InvalidCredentialsState)
            {
                LogoutFromZwift();
            }
        }

        public async Task CheckForNewVersion()
        {
            if (_haveCheckedVersion)
            {
                return;
            }

            _haveCheckedVersion = true;

            var currentVersion = System.Version.Parse(Version);
            var latestRelease = _versionChecker.GetLatestRelease();

            if (latestRelease.official.Version > currentVersion)
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
            if (latestRelease.official.Version > thisVersion)
            {
                return;
            }

            // If this version is newer than the previous one shown and it's
            // the latest version then show the what is new dialog
            if (thisVersion > previousVersion && thisVersion == latestRelease.official.Version)
            {
                // Update the current version so that the next time this won't be shown
                _userPreferences.Save();
                await _windowService.ShowWhatIsNewDialog(latestRelease.official);
            }
        }

        private void LogoutFromZwift()
        {
            ZwiftAccessToken = null;
            ZwiftAvatarUri = null;
            ZwiftName = null;
            LoggedInToZwift = false;
        }

        private static bool IsValidToken(string? accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            var token = new JsonWebToken(accessToken);

            // Token is at least valid for another hour
            return token.ValidTo > DateTime.UtcNow.AddHours(1);
        }

        private async Task<CommandResult> LoadRoute()
        {
            var fileName = await _windowService.ShowOpenFileDialog(_userPreferences.LastUsedFolder);

            if (!string.IsNullOrEmpty(fileName))
            {
                LoadRouteFromPath(fileName);
            }

            return CommandResult.Success();
        }

#pragma warning disable CS1998
        private Task<CommandResult> LaunchRouteBuilder()
#pragma warning restore CS1998
        {
            var assemblyLocation = GetType().Assembly.Location;
            var installationDirectory = Path.GetDirectoryName(assemblyLocation);

            if (!string.IsNullOrEmpty(installationDirectory))
            {
                var routeBuilderPath = Path.Combine(installationDirectory, "RoadCaptain.RouteBuilder.exe");

                if (File.Exists(routeBuilderPath))
                {
                    Process.Start(routeBuilderPath);

                    return Task.FromResult(CommandResult.Success());
                }
            }

            return Task.FromResult<CommandResult>(CommandResult.Failure("Could not locate RoadCaptain Route Builder"));
        }

        private CommandResult StartRoute()
        {
            if (Route?.PlannedRoute == null)
            {
                throw new InvalidOperationException("No route has been loaded");
            }

            _configuration.Route = RoutePath;
            
            _userPreferences.Route = RoutePath;
            _userPreferences.Save();

            _gameStateDispatcher.RouteSelected(Route!.PlannedRoute!);
            _gameStateDispatcher.StartRoute();

            return CommandResult.Success();
        }

        private async Task<CommandResult> LogInToZwift(Window window)
        {
            var tokenResponse = await _credentialCache.LoadAsync();
            
            tokenResponse ??= await _windowService.ShowLogInDialog(window);

            if (tokenResponse != null &&
                !string.IsNullOrEmpty(tokenResponse.AccessToken))
            {   
                ZwiftAccessToken = tokenResponse.AccessToken;
                
                if (tokenResponse.UserProfile != null)
                {
                    ZwiftName =
                        $"{tokenResponse.UserProfile.FirstName} {tokenResponse.UserProfile.LastName}";
                    ZwiftAvatarUri = tokenResponse.UserProfile.Avatar;
                    ZwiftAvatar = DownloadAvatarImage(ZwiftAvatarUri);
                }
                
                // Keep this in memory so that when the app navigates
                // from the in-game window to the main window the user
                // remains logged in.
                await _credentialCache.StoreAsync(tokenResponse);

                LoggedInToZwift = true;

                _gameStateDispatcher.LoggedIn();

                return CommandResult.Success();
            }

            return CommandResult.Aborted();
        }

        private static IImage? DownloadAvatarImage(string? uri)
        {
            if (uri != null && uri.StartsWith("https://"))
            {
                using var client = new HttpClient();
                try
                {
                    var imageBytes = client.GetByteArrayAsync(uri).GetAwaiter().GetResult();

                    return new Bitmap(new MemoryStream(imageBytes));
                }
                catch
                {
                    // Nop
                }
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (assets == null)
            {
                return null;
            }

            if (uri != null && uri.StartsWith("avares://"))
            {
                return new Bitmap(assets.Open(new Uri(uri)));
            }

            return new Bitmap(assets.Open(new Uri("avares://RoadCaptain.App.Shared/Assets/profile-default.png")));
        }

        private CommandResult OpenLink(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return CommandResult.Failure("Invalid url");
            }

            var startInfo = new ProcessStartInfo(uri.ToString())
            {
                UseShellExecute = true
            };

            Process.Start(startInfo);

            return CommandResult.Success();

        }

        public async Task Initialize()
        {
            var credentials = await _credentialCache.LoadAsync();

            if (credentials != null && IsValidToken(credentials.AccessToken))
            {
                ZwiftAccessToken = credentials.AccessToken;
                ZwiftAvatarUri = string.IsNullOrEmpty(credentials.UserProfile?.Avatar) ? "avares://RoadCaptain.App.Shared/Assets/profile-default.png" : credentials.UserProfile.Avatar;
                ZwiftAvatar = DownloadAvatarImage(ZwiftAvatarUri);
                ZwiftName = string.IsNullOrEmpty(credentials.UserProfile?.FirstName) ? "(stored token)" : credentials.UserProfile.FirstName + " " + credentials.UserProfile.LastName;
                LoggedInToZwift = true;
                
                _gameStateDispatcher.LoggedIn();
            }
            
        }
    }
}
