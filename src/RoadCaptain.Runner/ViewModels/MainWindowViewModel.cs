using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Microsoft.Win32;
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
        private readonly IComponentContext _componentContext;
        private bool _loggedInToZwift;
        private string _zwiftName;
        private string _zwiftAvatarUri;

        public MainWindowViewModel(
            ISegmentStore segmentStore, 
            IRouteStore routeStore, 
            IComponentContext componentContext,
            Configuration configuration)
        {
            _segmentStore = segmentStore;
            _routeStore = routeStore;
            _componentContext = componentContext;

            if (!string.IsNullOrEmpty(configuration.AccessToken))
            {
                ZwiftAccessToken = configuration.AccessToken;
                LoggedInToZwift = true;
            }

            if (!string.IsNullOrEmpty(configuration.Route))
            {
                RoutePath = configuration.Route;
            }
            else if (!string.IsNullOrEmpty(AppSettings.Default.Route))
            {
                RoutePath = AppSettings.Default.Route;
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
            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "JSON files (.json)|*.json",
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            
            if (result.HasValue && result.Value)
            {
                RoutePath = dialog.FileName;

                WindowTitle = $"RoadCaptain - {RoutePath}";
            }

            return CommandResult.Success();
        }

        private CommandResult StartRoute(Window window)
        {
            var inGameWindowModel = new InGameWindowModel(_segmentStore.LoadSegments());
            
            inGameWindowModel.InitializeRoute(_routeStore.LoadFrom(RoutePath));

            var configuration = _componentContext.Resolve<Configuration>();
            configuration.AccessToken = ZwiftAccessToken;
            configuration.Route = RoutePath;

            AppSettings.Default.Route = RoutePath;
            AppSettings.Default.Save();

            var viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, _segmentStore.LoadSegments());

            var inGameWindow = _componentContext.Resolve<InGameNavigationWindow>();
            inGameWindow.DataContext = viewModel;
            
            inGameWindow.Show();

            window.Close();

            return CommandResult.Success();
        }

        private CommandResult LogInToZwift(Window window)
        {
            var zwiftLoginWindow = _componentContext.Resolve<ZwiftLoginWindow>();
            
            zwiftLoginWindow.Owner = window;
            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            zwiftLoginWindow.ShowDialog();

            if (zwiftLoginWindow.TokenResponse != null &&
                !string.IsNullOrEmpty(zwiftLoginWindow.TokenResponse.AccessToken))
            {
                ZwiftAccessToken = zwiftLoginWindow.TokenResponse.AccessToken;
                if (zwiftLoginWindow.TokenResponse.UserProfile != null)
                {
                    ZwiftName =
                        $"{zwiftLoginWindow.TokenResponse.UserProfile.FirstName} {zwiftLoginWindow.TokenResponse.UserProfile.LastName}";
                    ZwiftAvatarUri = zwiftLoginWindow.TokenResponse.UserProfile.Avatar;
                }

                LoggedInToZwift = true;

                return CommandResult.Success();
            }

            return CommandResult.Aborted();
        }
    }
}