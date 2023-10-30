using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class ManageRoutesViewModel : ViewModelBase
    {
        private ImmutableList<string>? _repositories;
        private string? _selectedRepository;
        private readonly RetrieveRepositoryNamesUseCase _retrieveRepositoryNamesUseCase;
        private ImmutableList<Shared.ViewModels.RouteViewModel>? _routes;
        private readonly IWindowService _windowService;
        private readonly DeleteRouteUseCase _deleteRouteUseCase;

        public ManageRoutesViewModel(RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase, IWindowService windowService, DeleteRouteUseCase deleteRouteUseCase)
        {
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
            _windowService = windowService;
            _deleteRouteUseCase = deleteRouteUseCase;
        }

        public AsyncRelayCommand DeleteRouteCommand => new AsyncRelayCommand(
            parameter => DeleteRouteAsync((parameter as Shared.ViewModels.RouteViewModel)!),
            parameter => parameter is Shared.ViewModels.RouteViewModel);

        private async Task<CommandResult> DeleteRouteAsync(Shared.ViewModels.RouteViewModel parameter)
        {
            var result = await _windowService.ShowQuestionDialog(
                "Delete route?",
                $"Are you sure you want to delete the route {parameter.Name}?");

            if (result == MessageBoxResult.No)
            {
                return CommandResult.Aborted();    
            }

            try
            {
                await _deleteRouteUseCase.ExecuteAsync(new DeleteRouteCommand(parameter.Uri, parameter.RepositoryName));
            }
            catch (Exception e)
            {
                return CommandResult.Failure($"Failed to delete route: {e.Message}");
            }
            
            return CommandResult.Success();
        }

        public Task InitializeAsync()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute(new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Manage)).ToImmutableList();
            
            return Task.CompletedTask;
        }
        
        public ImmutableList<string> Repositories
        {
            get => _repositories ?? ImmutableList<string>.Empty;
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
        
        public ImmutableList<Shared.ViewModels.RouteViewModel> Routes
        {
            get => _routes ?? ImmutableList<Shared.ViewModels.RouteViewModel>.Empty;
            set
            {
                if (_routes != null && value == _routes)   
                {
                    return;
                }

                _routes = value;

                this.RaisePropertyChanged();
            }
        }
    }
}