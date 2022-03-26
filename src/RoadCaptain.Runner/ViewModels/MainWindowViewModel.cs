using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.Ports;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Commands;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _routePath;
        private string _windowTitle = "RoadCaptain";
        private readonly ISegmentStore _segmentStore;
        private readonly IRouteStore _routeStore;
        private readonly Configuration _configuration;
        private readonly AppSettings _appSettings;
        private bool _loggedInToZwift;
        private string _zwiftName;
        private string _zwiftAvatarUri;
        private readonly IWindowService _windowService;

        public MainWindowViewModel(
            ISegmentStore segmentStore, 
            IRouteStore routeStore,
            Configuration configuration, 
            AppSettings appSettings, 
            IWindowService windowService)
        {
            _segmentStore = segmentStore;
            _routeStore = routeStore;
            _configuration = configuration;
            _appSettings = appSettings;
            _windowService = windowService;

            if (!string.IsNullOrEmpty(configuration.AccessToken))
            {
                ZwiftAccessToken = configuration.AccessToken;
                ZwiftAvatarUri = "Assets/profile-default.png";
                ZwiftName = "(stored token)";
                LoggedInToZwift = true;
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
                if (value == _routePath) return;
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
                if (value == _windowTitle) return;
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public bool LoggedInToZwift
        {
            get => _loggedInToZwift;
            set
            {
                if (value == _loggedInToZwift) return;
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

        public string ZwiftAccessToken { get; private set; }

        public string ZwiftName
        {
            get => _zwiftName;
            set
            {
                if (value == _zwiftName) return;
                _zwiftName = value;
                OnPropertyChanged();
            }
        }

        public string ZwiftAvatarUri
        {
            get => _zwiftAvatarUri;
            set
            {
                if (value == _zwiftAvatarUri) return;
                _zwiftAvatarUri = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartRouteCommand { get; set; }
        public ICommand LoadRouteCommand { get; set; }
        public ICommand LogInCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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
                catch { /* nop */ }

                WindowTitle = $"RoadCaptain - {routeFileName}";
            }

            return CommandResult.Success();
        }

        private CommandResult StartRoute(Window window)
        {
            var inGameWindowModel = new InGameWindowModel(_segmentStore.LoadSegments());
            
            inGameWindowModel.InitializeRoute(_routeStore.LoadFrom(RoutePath));
            
            _configuration.AccessToken = ZwiftAccessToken;
            _configuration.Route = RoutePath;

            _appSettings.Route = RoutePath;
            _appSettings.Save();

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

                return CommandResult.Success();
            }

            return CommandResult.Aborted();
        }
    }
}