// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class OpenRouteDialogViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;
        private readonly IRouteStore _routeStore;
        private RouteModel? _selectedRoute;
        private string? _routeFilePath;
        private string? _selectedRouteName;

        public OpenRouteDialogViewModel(IWindowService windowService, IUserPreferences userPreferences,
            IRouteStore routeStore)
        {
            _windowService = windowService;
            _userPreferences = userPreferences;
            _routeStore = routeStore;

            SelectRouteCommand = new AsyncRelayCommand(
                async _ => await SelectRoute(),
                _ => true);

            SelectFileCommand = new AsyncRelayCommand(
                async _ => await SelectFile(),
                _ => true);

            OpenRouteCommand = new AsyncRelayCommand(
                    async _ => OpenRoute(),
                    _ => SelectedRoute != null || !string.IsNullOrEmpty(RouteFilePath))
                .OnSuccess(async _ => { await CloseWindow(); })
                .OnFailure(async result =>
                    await _windowService.ShowErrorDialog($"Unable to save route: {result.Message}", null))
                .SubscribeTo(this, () => SelectedRoute)
                .SubscribeTo(this, () => RouteFilePath);
        }

        private async Task<CommandResult> SelectFile()
        {
            var filePath = await _windowService.ShowOpenFileDialog(
                _userPreferences.LastUsedFolder,
                new Dictionary<string, string>
                {
                    { "json", "RoadCaptain route file (.json)" },
                    { "gpx", "GPS Exchange Format (.gpx)" }
                });

            if (string.IsNullOrEmpty(filePath))
            {
                return CommandResult.Aborted();
            }

            RouteFilePath = filePath;
            SelectedRouteName = Path.GetFileName(_routeFilePath);

            return CommandResult.Success();
        }

        public string? RouteFilePath
        {
            get => _routeFilePath;
            set
            {
                if (value == _routeFilePath) return;

                _routeFilePath = value;

                this.RaisePropertyChanged();
            }
        }

        public string? SelectedRouteName
        {
            get => _selectedRouteName;
            set
            {
                if (value == _selectedRouteName) return;

                _selectedRouteName = value;

                this.RaisePropertyChanged();
            }
        }

        public RouteModel? SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                if (value == _selectedRoute) return;

                _selectedRoute = value;
                SelectedRouteName = _selectedRoute?.Name;

                this.RaisePropertyChanged();
            }
        }

        public ICommand SelectRouteCommand { get; }
        public ICommand SelectFileCommand { get; }
        public ICommand OpenRouteCommand { get; }

        private async Task<CommandResult> SelectRoute()
        {
            var selectedRoute = await _windowService.ShowSelectRouteDialog();

            if (selectedRoute != null)
            {
                SelectedRoute = selectedRoute;
                return CommandResult.Success();
            }

            return CommandResult.Aborted();
        }

        public event EventHandler? ShouldClose;

        private CommandResult OpenRoute()
        {
            if (SelectedRoute == null && string.IsNullOrEmpty(RouteFilePath))
            {
                return CommandResult.Failure(
                    "No local file or route from a repository selected, I can't open a route that way");
            }

            return CommandResult.Success();
        }

        private Task CloseWindow()
        {
            ShouldClose?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
