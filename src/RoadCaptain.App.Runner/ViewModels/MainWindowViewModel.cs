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
using Newtonsoft.Json;
using ReactiveUI;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.UserPreferences;
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
        private RouteModel _route = new();
        private readonly IVersionChecker _versionChecker;
        private readonly ISegmentStore _segmentStore;
        private bool _haveCheckedVersion;
        private IImage? _zwiftAvatar;
        private bool _endActivityAtEndOfRoute;
        private bool _loopRouteAtEndOfRoute;

        public MainWindowViewModel(Configuration configuration,
            IUserPreferences userPreferences,
            IWindowService windowService,
            IGameStateDispatcher gameStateDispatcher,
            IRouteStore routeStore,
            IVersionChecker versionChecker,
            ISegmentStore segmentStore)
        {
            _configuration = configuration;
            _userPreferences = userPreferences;
            _windowService = windowService;
            _gameStateDispatcher = gameStateDispatcher;
            _routeStore = routeStore;
            _versionChecker = versionChecker;
            _segmentStore = segmentStore;

            if (IsValidToken(configuration.AccessToken))
            {
                ZwiftAccessToken = configuration.AccessToken;
                ZwiftAvatarUri = "avares://RoadCaptain.App.Shared/Assets/profile-default.png";
                ZwiftAvatar = DownloadAvatarImage(ZwiftAvatarUri);
                ZwiftName = "(stored token)";
                LoggedInToZwift = true;
                _gameStateDispatcher.Dispatch(new LoggedInState(ZwiftAccessToken));
            }

            if (!string.IsNullOrEmpty(configuration.Route))
            {
                RoutePath = configuration.Route;
                var plannedRoute = _routeStore.LoadFrom(RoutePath);
                if (plannedRoute != null)
                {
                    Route = RouteModel.From(plannedRoute, _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport));
                }
                else
                {
                    Route = new RouteModel();
                }
            }
            else if (!string.IsNullOrEmpty(userPreferences.Route))
            {
                RoutePath = userPreferences.Route;
                var plannedRoute = _routeStore.LoadFrom(RoutePath);
                if (plannedRoute != null)
                {
                    Route = RouteModel.From(plannedRoute, _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport));
                }
                else
                {
                    Route = new RouteModel();
                }
            }

            StartRouteCommand = new RelayCommand(
                    _ => StartRoute(),
                    _ => CanStartRoute
                )
                .SubscribeTo(this, () => CanStartRoute);

            LoadRouteCommand = new AsyncRelayCommand(
                _ => LoadRoute(),
                _ => true);

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

        public RouteModel Route
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

        public bool LoopRouteAtEndOfRoute
        {
            get => _loopRouteAtEndOfRoute;
            set
            {
                if (value == _loopRouteAtEndOfRoute) return;
                _loopRouteAtEndOfRoute = value;
                // When doing a loop we can't automatically end the activity...
                EndActivityAtEndOfRoute = false;
                _userPreferences.LoopRouteAtEndOfRoute = value;
                this.RaisePropertyChanged();
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
            var fileName = await _windowService.ShowOpenFileDialog(_userPreferences.LastUsedFolder);

            if (!string.IsNullOrEmpty(fileName))
            {
                RoutePath = fileName;

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

                var plannedRoute = _routeStore.LoadFrom(RoutePath);
                if (plannedRoute != null)
                {
                    Route = RouteModel.From(plannedRoute, _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport));
                }
                else
                {
                    Route = new RouteModel();
                }
            }

            return CommandResult.Success();
        }

#pragma warning disable CS1998
        private async Task<CommandResult> LaunchRouteBuilder()
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

                    return CommandResult.Success();
                }
            }

            return CommandResult.Failure("Could not locate RoadCaptain Route Builder");
        }

        private CommandResult StartRoute()
        {
            _configuration.Route = RoutePath;

            _userPreferences.Route = RoutePath;
            _userPreferences.Save();

            if (Route.PlannedRoute == null)
            {
                var plannedRoute = _routeStore.LoadFrom(RoutePath);
                Route = RouteModel.From(plannedRoute, _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport));
            }

            _gameStateDispatcher.RouteSelected(Route.PlannedRoute);

            _gameStateDispatcher.Dispatch(new WaitingForConnectionState());

            return CommandResult.Success();
        }

        private async Task<CommandResult> LogInToZwift(Window window)
        {
            TokenResponse? tokenResponse = null;

#if DEBUG
            // This is for testing only to prevent me having to log in all the time.
            if (File.Exists("devtokens.json"))
            {
                tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await File.ReadAllTextAsync("devtokens.json"));

                if (tokenResponse?.AccessToken != null && new JsonWebToken(tokenResponse.AccessToken).ValidTo < DateTime.Now)
                {
                    // When the token expires, break here and use postman to refresh the token manually
                    Debugger.Break();
                    tokenResponse = null;
                }
            }
#endif
            
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

#if DEBUG
                // This is for testing only to prevent me having to log in all the time.
                if (tokenResponse.UserProfile.FirstName + " " + tokenResponse.UserProfile.LastName ==
                    "Sander van Vliet [RoadCaptain]")
                {
                    await File.WriteAllTextAsync("devtokens.json", JsonConvert.SerializeObject(tokenResponse));
                }
#endif

                LoggedInToZwift = true;

                _gameStateDispatcher.Dispatch(new LoggedInState(ZwiftAccessToken));

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
    }
}