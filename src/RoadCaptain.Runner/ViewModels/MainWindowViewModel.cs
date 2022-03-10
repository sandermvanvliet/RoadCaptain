using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using RoadCaptain.Ports;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Commands;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IRouteStore _routeStore;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IRouteStore routeStore, ISegmentStore segmentStore)
        {
            _routeStore = routeStore;

            var segments = segmentStore.LoadSegments();

            Model = new MainWindowModel(segments);

            OpenRouteCommand = new RelayCommand(
                    _ => OpenRoute(),
                    _ => true);
        }

        public ICommand OpenRouteCommand { get; }
        public MainWindowModel Model { get; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CommandResult OpenRoute()
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

            if (!result.HasValue || !result.Value)
            {
                return CommandResult.Success();
            }

            try
            {
                Model.InitializeRoute(_routeStore.LoadFrom(dialog.FileName));

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
        }
    }
}