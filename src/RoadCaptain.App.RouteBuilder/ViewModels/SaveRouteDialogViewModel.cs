// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.ViewModels;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class SaveRouteDialogViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private RouteViewModel _route;
        private ImmutableList<string>? _repositoryNames;
        private string? _selectedRepositoryName;
        private readonly SaveRouteUseCase _saveRouteUseCase;
        private readonly IUserPreferences _userPreferences;
        private readonly IEnumerable<IRouteRepository> _repositories;
        private string? _outputFilePath;
    
        public SaveRouteDialogViewModel(
            IWindowService windowService,
            RouteViewModel route,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase, 
            SaveRouteUseCase saveRouteUseCase, 
            IUserPreferences userPreferences,
            IEnumerable<IRouteRepository> repositories)
        {
            _windowService = windowService;
            _route = route;
            _saveRouteUseCase = saveRouteUseCase;
            _userPreferences = userPreferences;
            _repositories = repositories;
            Repositories = retrieveRepositoryNamesUseCase.Execute(new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Store)).ToImmutableList();
            SelectedRepositoryName = route.RepositoryName;
        }

        public ICommand SaveRouteCommand => new AsyncRelayCommand(
                _ => SaveRoute(),
                _ => !string.IsNullOrEmpty(RouteName) &&
                    (SelectedRepositoryName != null || (SelectedRepositoryName == null && !string.IsNullOrEmpty(OutputFilePath))))
            .OnSuccess(async _ =>
            {
                await CloseWindow();
            })
            .OnFailure(async result =>
                await _windowService.ShowErrorDialog($"Unable to save route: {result.Message}", _windowService.GetCurrentWindow()!))
            .SubscribeTo(this, () => SelectedRepositoryName)
            .SubscribeTo(this, () => RouteName)
            .SubscribeTo(this, () => OutputFilePath);
        
        public ICommand SelectFileCommand => new AsyncRelayCommand(
            _ => SelectFile(),
            _ => !string.IsNullOrEmpty(RouteName))
            .OnSuccess(_ =>
            {
                SelectedRepositoryName = null;
                return Task.CompletedTask;
            })
            .OnFailure(async result =>
                await _windowService.ShowErrorDialog($"Unable to select a file to save to: {result.Message}", _windowService.GetCurrentWindow()!))
            .SubscribeTo(this, () => RouteName);
        
        private async Task<CommandResult> SaveRoute()
        {
            if (string.IsNullOrEmpty(RouteName))
            {
                return CommandResult.Failure("Route name is empty");
            }
            if (string.IsNullOrEmpty(SelectedRepositoryName) && string.IsNullOrEmpty(OutputFilePath))
            {
                return CommandResult.Failure("No route repository selected and no local file given, can't save this route without either of those");
            }
            
            try
            {
                if(!string.IsNullOrEmpty(SelectedRepositoryName))
                {
                    var repository = _repositories.SingleOrDefault(r => r.Name == SelectedRepositoryName);

                    if (repository == null)
                    {
                        throw new InvalidOperationException(
                            "I don't know what happened but a repository name was selected that I can't find...");
                    }
                }

                var uri = _route.Uri;

                // If the user selects a different repository we need to clear the 
                // route URI because otherwise we'd attempt to update it...
                if (SelectedRepositoryName != _route.RepositoryName)
                {
                    uri = null;
                }
                
                var routeUri = await _saveRouteUseCase.ExecuteAsync(new SaveRouteCommand(_route.AsPlannedRoute()!, RouteName, SelectedRepositoryName, OutputFilePath, uri));
                
                // Populate the URI and repository name onto the route as if we loaded it from there
                _route.Save(routeUri, SelectedRepositoryName);
                
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
        }

        private async Task<CommandResult> SelectFile()
        {
            var outputFilePath = await _windowService.ShowSaveFileDialog(_userPreferences.LastUsedFolder, RouteName + ".json");

            if (string.IsNullOrEmpty(outputFilePath))
            {
                return CommandResult.Aborted();
            }

            OutputFilePath = outputFilePath;
            
            return CommandResult.Success();
        }

        public string? OutputFilePath
        {
            get => _outputFilePath;
            set => SetProperty(ref _outputFilePath, value);
        }

        public string? RouteName
        {
            get => _route.Name;
            set
            {
                _route.Name = value;
                OnPropertyChanged();
            }
        }

        public RouteViewModel Route
        {
            get => _route;
            set => SetProperty(ref _route, value);
        }

        public ImmutableList<string> Repositories
        {
            get => _repositoryNames ?? ImmutableList<string>.Empty;
            set => SetProperty(ref _repositoryNames, value);
        }

        public string? SelectedRepositoryName
        {
            get => _selectedRepositoryName;
            set
            {
                SetProperty(ref _selectedRepositoryName, value);
                
                if (value != null)
                {
                    OutputFilePath = null;
                }
            }
        }
        public event EventHandler? ShouldClose;

        private Task CloseWindow()
        {
            ShouldClose?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public void Initialize()
        {
        }
    }
}
