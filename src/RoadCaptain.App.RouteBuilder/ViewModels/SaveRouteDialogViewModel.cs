// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    internal class SaveRouteDialogViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;
        private RouteViewModel _route;

        public SaveRouteDialogViewModel(IWindowService windowService, IUserPreferences userPreferences, RouteViewModel route)
        {
            _windowService = windowService;
            _userPreferences = userPreferences;
            _route = route;

            SaveRouteCommand = new AsyncRelayCommand(
                _ => SaveRoute(),
                _ => true)
                .OnSuccess(async _ =>
                {
                    await CloseWindow();
                });

            SelectPathCommand = new AsyncRelayCommand(
                _ => SelectPath(),
                _ => true);
        }

        private async Task<CommandResult> SelectPath()
        {
            var result = await _windowService.ShowSaveFileDialog(_userPreferences.LastUsedFolder, RouteName);

            if (!string.IsNullOrEmpty(result))
            {
                Path = result;
                return CommandResult.Success();
            }

            return CommandResult.Aborted();
        }


        private Task<CommandResult> SaveRoute()
        {
            try
            {
                _route.Save();

                return Task.FromResult(CommandResult.Success());
            }
            catch (Exception e)
            {
                return Task.FromResult<CommandResult>(CommandResult.Failure(e.Message));
            }
        }

        public string? RouteName
        {
            get => _route.Name;
            set
            {
                if (_route.Name == value) return;
                _route.Name = value ?? string.Empty;
                this.RaisePropertyChanged();
            }
        }

        public RouteViewModel Route
        {
            get => _route;
            set
            {
                if (_route == value) return;
                _route = value;
                this.RaisePropertyChanged();
            }
        }

        public string? Path
        {
            get => _route.OutputFilePath;
            set
            {
                if (_route.OutputFilePath == value) return;
                _route.OutputFilePath = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand SaveRouteCommand { get; }
        public ICommand SelectPathCommand { get; }
        public event EventHandler? ShouldClose;

        private Task CloseWindow()
        {
            ShouldClose?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
