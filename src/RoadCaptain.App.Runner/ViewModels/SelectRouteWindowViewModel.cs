using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class SelectRouteWindowViewModel : ViewModelBase
    {
        private readonly SearchRoutesUseCase _useCase;
        private readonly RetrieveRepositoryNamesUseCase _retrieveRepositoryNamesUseCase;
        private readonly IWindowService _windowService;
        private readonly IWorldStore _worldStore;
        private RouteViewModel[] _routes = Array.Empty<RouteViewModel>();
        private string[] _repositories = Array.Empty<string>();
        private RouteViewModel? _selectedRoute;
        private World[] _availableWorlds = Array.Empty<World>();
        private World? _filterWorld;

        public SelectRouteWindowViewModel(SearchRoutesUseCase useCase,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase,
            IWindowService windowService, IWorldStore worldStore)
        {
            _useCase = useCase;
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
            _windowService = windowService;
            _worldStore = worldStore;
        }

        public AsyncRelayCommand RefreshRoutesCommand => new AsyncRelayCommand(
                async parameter => await LoadRoutesForRepositoryAsync(parameter as string ?? "(unknown)"),
                _ => true)
            .OnFailure(async _ =>
            {
                await _windowService.ShowErrorDialog(_.Message);
                Routes = Array.Empty<RouteViewModel>();
            })
            .OnSuccess(_ => SelectedRoute = null);

        public string WindowTitle => "RoadCaptain - Route selection";

        public void Initialize()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute();
            var allWorlds = new World { Id = "all", Name = "All" };
            AvailableWorlds = new[] { allWorlds }
                .Concat(_worldStore.LoadWorlds())
                .ToArray();
            FilterWorld = allWorlds;
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

        public World[] AvailableWorlds
        {
            get => _availableWorlds;
            set
            {
                if (value == _availableWorlds)
                {
                    return;
                }
                
                _availableWorlds = value;

                this.RaisePropertyChanged();
            }
        }

        public World? FilterWorld
        {
            get => _filterWorld;
            set
            {
                if (value == _filterWorld)
                {
                    return;
                }
                
                _filterWorld = value;
                
                this.RaisePropertyChanged();
            }
        }

        public async Task<CommandResult> LoadRoutesForRepositoryAsync(string repository)
        {
            try
            {
                var command = new SearchRouteCommand(repository);

                Routes = (await _useCase.ExecuteAsync(command))
                    .Select(routeModel => new RouteViewModel(routeModel))
                    .ToArray();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }

            return CommandResult.Success();
        }
    }
}