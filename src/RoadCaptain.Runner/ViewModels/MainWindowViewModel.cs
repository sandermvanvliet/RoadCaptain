using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using RoadCaptain.Ports;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Commands;

namespace RoadCaptain.Runner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IRouteStore _routeStore;
        private readonly ISegmentStore _segmentStore;
        private string _routePath;
        private string _windowTitle = "RoadCaptain";
        private string _zwiftPassword;
        private string _zwiftUsername;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            _routeStore = routeStore;
            _segmentStore = segmentStore;

            StartRouteCommand = new RelayCommand(
                _ => CommandResult.Aborted(),
                _ => CanStartRoute
            );

            LoadRouteCommand = new RelayCommand(
                _ => LoadRoute(),
                _ => true);
        }

        public bool CanStartRoute
        {
            get => !string.IsNullOrEmpty(RoutePath) &&
                   !string.IsNullOrEmpty(ZwiftUsername) &&
                   !string.IsNullOrEmpty(ZwiftPassword);
        }

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

        public string ZwiftUsername
        {
            get => _zwiftUsername;
            set
            {
                if (value == _zwiftUsername) return;
                _zwiftUsername = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartRoute));
            }
        }

        public string ZwiftPassword
        {
            get => _zwiftPassword;
            set
            {
                if (value == _zwiftPassword) return;
                _zwiftPassword = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartRoute));
            }
        }

        public ICommand StartRouteCommand { get; set; }
        public ICommand LoadRouteCommand { get; set; }

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
    }
}