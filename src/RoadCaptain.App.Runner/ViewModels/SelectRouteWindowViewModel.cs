using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class SelectRouteWindowViewModel : ViewModelBase
    {
        private readonly SearchRoutesUseCase _useCase;
        private readonly RetrieveRepositoryNamesUseCase _retrieveRepositoryNamesUseCase;
        private RouteViewModel[] _routes = Array.Empty<RouteViewModel>();
        private string[] _repositories = Array.Empty<string>();
        private RouteViewModel? _selectedRoute;

        public SelectRouteWindowViewModel(
            SearchRoutesUseCase useCase,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase)
        {
            _useCase = useCase;
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
        }
        
        public void Initialize()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute();
        }

        public RouteViewModel[] Routes
        {
            get => _routes;
            set
            {
                if (value == _routes)
                {
                    return;
                }
                
                _routes = value;
                
                this.RaisePropertyChanged();
            }
        }

        public string[] Repositories
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

        public RouteViewModel? SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                if (value == _selectedRoute)
                {
                    return;
                }

                _selectedRoute = value;
                
                this.RaisePropertyChanged();
            }
        }

        public async Task LoadRoutesForRepositoryAsync(string repository)
        {
            var command = new SearchRouteCommand(repository);
            
            await _useCase.ExecuteAsync(command);
        }
    }
}