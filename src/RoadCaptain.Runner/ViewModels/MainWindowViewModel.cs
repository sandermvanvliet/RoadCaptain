using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.IdentityModel.JsonWebTokens;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Commands;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _appSettings;
        private readonly Configuration _configuration;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private readonly IRouteStore _routeStore;
        private readonly ISegmentStore _segmentStore;
        private readonly IWindowService _windowService;
        private bool _loggedInToZwift;
        private string _routePath;
        private string _windowTitle = "RoadCaptain";
        private string _zwiftAvatarUri;
        private string _zwiftName;

        public MainWindowViewModel(ISegmentStore segmentStore,
            IRouteStore routeStore,
            Configuration configuration,
            AppSettings appSettings,
            IWindowService windowService,
            IGameStateDispatcher gameStateDispatcher)
        {
            _segmentStore = segmentStore;
            _routeStore = routeStore;
            _configuration = configuration;
            _appSettings = appSettings;
            _windowService = windowService;
            _gameStateDispatcher = gameStateDispatcher;

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
            }
            else if (!string.IsNullOrEmpty(appSettings.Route))
            {
                RoutePath = appSettings.Route;
            }

            StartRouteCommand = new RelayCommand(
                _ => StartRoute(_ as Window),
                _ => CanStartRoute
            );

            LoadRouteCommand = new RelayCommand(
                _ => LoadRoute(),
                _ => true);

            LogInCommand = new RelayCommand(
                _ => LogInToZwift(_ as Window),
                _ => !LoggedInToZwift);
        }

        public bool CanStartRoute =>
            !string.IsNullOrEmpty(RoutePath) &&
            LoggedInToZwift;

        public string RoutePath
        {
            get => _routePath;
            set
            {
                if (value == _routePath)
                {
                    return;
                }

                _routePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartRoute));
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
                OnPropertyChanged();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZwiftLoggedInText));
                OnPropertyChanged(nameof(CanStartRoute));
            }
        }

        public string ZwiftLoggedInText =>
            LoggedInToZwift
                ? "Logged in to Zwift"
                : "Not yet logged in to Zwift";

        public string ZwiftAccessToken { get; set; }

        public string ZwiftName
        {
            get => _zwiftName;
            set
            {
                if (value == _zwiftName)
                {
                    return;
                }

                _zwiftName = value;
                OnPropertyChanged();
            }
        }

        public string ZwiftAvatarUri
        {
            get => _zwiftAvatarUri;
            set
            {
                if (value == _zwiftAvatarUri)
                {
                    return;
                }

                _zwiftAvatarUri = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartRouteCommand { get; set; }
        public ICommand LoadRouteCommand { get; set; }
        public ICommand LogInCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateGameState(object state)
        {
            if (state is InvalidCredentialsState)
            {
                LogoutFromZwift();
            }
        }

        private void LogoutFromZwift()
        {
            ZwiftAccessToken = null;
            ZwiftAvatarUri = null;
            ZwiftName = null;
            LoggedInToZwift = false;
        }

        private static bool IsValidToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            var token = new JsonWebToken(accessToken);

            // Token is at least valid for another hour
            return token.ValidTo > DateTime.UtcNow.AddHours(1);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CommandResult LoadRoute()
        {
            var fileName = _windowService.ShowOpenFileDialog();

            if (!string.IsNullOrEmpty(fileName))
            {
                RoutePath = fileName;

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
            }

            return CommandResult.Success();
        }

        private CommandResult StartRoute(Window window)
        {
            _configuration.Route = RoutePath;

            _appSettings.Route = RoutePath;
            _appSettings.Save();

            var inGameWindowModel = new InGameWindowModel(_segmentStore.LoadSegments());

            inGameWindowModel.InitializeRoute(_routeStore.LoadFrom(RoutePath));

            var viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, _segmentStore.LoadSegments());

            _windowService.ShowInGameWindow(window, viewModel);

            return CommandResult.Success();
        }

        private CommandResult LogInToZwift(Window window)
        {
            var tokenResponse = _windowService.ShowLogInDialog(window);

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
    }
}