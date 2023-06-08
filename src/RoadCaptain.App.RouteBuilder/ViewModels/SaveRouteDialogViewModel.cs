// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    internal class SaveRouteDialogViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserPreferences _userPreferences;
        private RouteViewModel _route;
        private ImmutableList<string> _repositories;
        private string? _selectedRepository;
        private readonly RetrieveRepositoryNamesUseCase _retrieveRepositoryNamesUseCase;
        private readonly SaveRouteUseCase _saveRouteUseCase;

        public SaveRouteDialogViewModel(
            IWindowService windowService,
            IUserPreferences userPreferences,
            RouteViewModel route,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase, 
            SaveRouteUseCase saveRouteUseCase)
        {
            _windowService = windowService;
            _userPreferences = userPreferences;
            _route = route;
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
            _saveRouteUseCase = saveRouteUseCase;
        }

        public ICommand SaveRouteCommand => new AsyncRelayCommand(
                _ => SaveRoute(),
                _ => SelectedRepository != null && !string.IsNullOrEmpty(RouteName))
            .OnSuccess(async _ =>
            {
                await CloseWindow();
            })
            .OnFailure(async result =>
                await _windowService.ShowErrorDialog($"Unable to save route: {result.Message}", null))
            .SubscribeTo(this, () => SelectedRepository)
            .SubscribeTo(this, () => RouteName);
        
        private async Task<CommandResult> SaveRoute()
        {
            if (string.IsNullOrEmpty(RouteName))
            {
                return CommandResult.Failure("Route name is empty");
            }
            if (string.IsNullOrEmpty(SelectedRepository))
            {
                return CommandResult.Failure("No route repository selected");
            }
            
            try
            {
                await _saveRouteUseCase.ExecuteAsync(new SaveRouteCommand(_route.AsPlannedRoute()!, RouteName, SelectedRepository));
                
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
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

        public ImmutableList<string> Repositories
        {
            get => _repositories;
            set
            {
                if (value == _repositories)
                {
                    return;
                }
                
                _repositories = value;
                
                this.RaisePropertyChanged();
            }
        }

        public string? SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (value == _selectedRepository)
                {
                    return;
                }
                
                _selectedRepository = value;
                
                this.RaisePropertyChanged();
            }
        }
        public event EventHandler? ShouldClose;

        private Task CloseWindow()
        {
            ShouldClose?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public async Task Initialize()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute(new RetrieveRepositoryNameCommand(RetrieveRepositoriesIntent.Store)).ToImmutableList();
        }
    }
}
