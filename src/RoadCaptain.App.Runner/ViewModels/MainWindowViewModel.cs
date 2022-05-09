using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.IdentityModel.JsonWebTokens;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUserPreferences _appSettings;
        private readonly Configuration _configuration;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private readonly IWindowService _windowService;
        private bool _loggedInToZwift;
        private string? _routePath;
        private string _windowTitle = "RoadCaptain";
        private string? _zwiftAvatarUri = "Assets/profile-default.png";
        private string? _zwiftName;
        private readonly LoadRouteUseCase _loadRouteUseCase;
        private readonly IRouteStore _routeStore;
        private string _version = "0.0.0.0";
        private string? _changelogUri;
        private PlannedRoute? _route;
        private readonly IVersionChecker _versionChecker;
        private bool _haveCheckedVersion;

        public MainWindowViewModel(Configuration configuration,
            IUserPreferences appSettings,
            IWindowService windowService,
            IGameStateDispatcher gameStateDispatcher,
            LoadRouteUseCase loadRouteUseCase,
            IRouteStore routeStore, 
            IVersionChecker versionChecker)
        {
            _configuration = configuration;
            _appSettings = appSettings;
            _windowService = windowService;
            _gameStateDispatcher = gameStateDispatcher;
            _loadRouteUseCase = loadRouteUseCase;
            _routeStore = routeStore;
            _versionChecker = versionChecker;

            if (IsValidToken(configuration.AccessToken))
            {
                ZwiftAccessToken = configuration.AccessToken;
                ZwiftAvatarUri = "Assets/profile-default.png";
                ZwiftName = "(stored token)";
                LoggedInToZwift = true;
                _gameStateDispatcher.Dispatch(new LoggedInState(ZwiftAccessToken));
            }

            if (!string.IsNullOrEmpty(configuration.Route))
            {
                RoutePath = configuration.Route;
                Route = _routeStore.LoadFrom(RoutePath);
            }
            else if (!string.IsNullOrEmpty(appSettings.Route))
            {
                RoutePath = appSettings.Route;
                Route = _routeStore.LoadFrom(RoutePath);
            }

            StartRouteCommand = new RelayCommand(
                _ => StartRoute(),
                _ => CanStartRoute
            );

            LoadRouteCommand = new AsyncRelayCommand(
                _ => LoadRoute(),
                _ => true);

            LogInCommand = new AsyncRelayCommand(
                _ => LogInToZwift(_ as Window),
                _ => !LoggedInToZwift);

            BuildRouteCommand = new AsyncRelayCommand(
                    _ => LaunchRouteBuilder(),
                    _ => true)
                .OnFailure(async result => await _windowService.ShowErrorDialog(result.Message, null));

            OpenLinkCommand = new RelayCommand(
                _ => OpenLink(_ as string),
                _ => !string.IsNullOrEmpty(_ as string));

            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";

            RebelRoutes = LoadRebelRoutes();
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
            Route != null &&
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

        public string ChangelogUri
        {
            get => _changelogUri;
            set
            {
                if (value == _changelogUri) return;
                _changelogUri = value;
                this.RaisePropertyChanged();
            }
        }

        public PlannedRoute? Route
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

        public List<PlannedRoute> RebelRoutes
        {
            get;
            private set;
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

            if (latestRelease != null && latestRelease.Version > currentVersion)
            {
                await _windowService.ShowNewVersionDialog(latestRelease);
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
            var fileName = await _windowService.ShowOpenFileDialog(_appSettings.LastUsedFolder);

            if (!string.IsNullOrEmpty(fileName))
            {
                RoutePath = fileName;

                _appSettings.LastUsedFolder = Path.GetDirectoryName(RoutePath);
                _appSettings.Save();

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
                
                Route = _routeStore.LoadFrom(RoutePath);
            }

            return CommandResult.Success();
        }

        private async Task<CommandResult> LaunchRouteBuilder()
        {
            var assemblyLocation = GetType().Assembly.Location;
            var installationDirectory = Path.GetDirectoryName(assemblyLocation);
            var routeBuilderPath = Path.Combine(installationDirectory, "RoadCaptain.RouteBuilder.exe");

            if (File.Exists(routeBuilderPath))
            {
                Process.Start(routeBuilderPath);

                return CommandResult.Success();
            }

            return CommandResult.Failure("Could not locate RoadCaptain Route Builder");
        }

        private CommandResult StartRoute()
        {
            _configuration.Route = RoutePath;

            _appSettings.Route = RoutePath;
            _appSettings.Save();

            if (Route != null)
            {
                _gameStateDispatcher.RouteSelected(Route);
            }
            else
            {
                _loadRouteUseCase.Execute(new LoadRouteCommand { Path = RoutePath });
            }

            _gameStateDispatcher.Dispatch(new WaitingForConnectionState());

            return CommandResult.Success();
        }

        private async Task<CommandResult> LogInToZwift(Window window)
        {
            var tokenResponse = await _windowService.ShowLogInDialog(window);

            if (tokenResponse != null &&
                !string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                ZwiftAccessToken = tokenResponse.AccessToken;
                if (tokenResponse.UserProfile != null)
                {
                    ZwiftName =
                        $"{tokenResponse.UserProfile.FirstName} {tokenResponse.UserProfile.LastName}";
                    ZwiftAvatarUri = tokenResponse.UserProfile.Avatar;
                }

                LoggedInToZwift = true;

                _gameStateDispatcher.Dispatch(new LoggedInState(ZwiftAccessToken));

                return CommandResult.Success();
            }

            return CommandResult.Aborted();
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
    }
}