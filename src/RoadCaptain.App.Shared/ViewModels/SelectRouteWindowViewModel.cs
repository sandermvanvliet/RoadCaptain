// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.Dialogs.ViewModels;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Shared.ViewModels
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
        private string? _filterRouteName;
        private string? _filterCreatorName;
        private string? _filterZwiftRouteName;
        private int? _filterDistanceMax;
        private int? _filterDescentMax;
        private int? _filterAscentMax;
        private int? _filterDistanceMin;
        private int? _filterAscentMin;
        private int? _filterDescentMin;
        private bool _isLoopYesChecked;
        private bool _isLoopNoChecked;
        private bool _isLoopBothChecked = true;
        private string? _filterRepository;
        private bool _isBusy;

        public SelectRouteWindowViewModel(SearchRoutesUseCase useCase,
            RetrieveRepositoryNamesUseCase retrieveRepositoryNamesUseCase,
            IWindowService windowService, IWorldStore worldStore)
        {
            _useCase = useCase;
            _retrieveRepositoryNamesUseCase = retrieveRepositoryNamesUseCase;
            _windowService = windowService;
            _worldStore = worldStore;
        }

        public AsyncRelayCommand SearchRoutesCommand => new AsyncRelayCommand(
                async parameter =>
                {
                    IsBusy = true;
                    return await LoadRoutesForRepositoryAsync(parameter as string ?? "(unknown)");
                },
                _ => true)
            .OnFailure(async _ =>
            {
                await _windowService.ShowErrorDialog(_.Message);
                Routes = Array.Empty<RouteViewModel>();
                IsBusy = false;
            })
            .OnSuccess(_ =>
            {  
                SelectedRoute = null;
                IsBusy = false;
            });

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string WindowTitle => "RoadCaptain - Route selection";

        public void Initialize()
        {
            Repositories = _retrieveRepositoryNamesUseCase.Execute(new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Retrieve));
            var allWorlds = new World { Id = "all", Name = "All" };
            AvailableWorlds = new[] { allWorlds }
                .Concat(_worldStore.LoadWorlds())
                .ToArray();
            FilterWorld = allWorlds;
            FilterRepository = "All";
        }

        public RouteViewModel[] Routes
        {
            get => _routes;
            set => SetProperty(ref _routes, value);
        }

        public string[] Repositories
        {
            get => _repositories;
            set => SetProperty(ref _repositories, value);
        }

        public RouteViewModel? SelectedRoute
        {
            get => _selectedRoute;
            set => SetProperty(ref _selectedRoute, value);
        }

        public World[] AvailableWorlds
        {
            get => _availableWorlds;
            set => SetProperty(ref _availableWorlds, value);
        }

        public string? FilterRepository
        {
            get => _filterRepository;
            set => SetProperty(ref _filterRepository, value);
        }

        public World? FilterWorld
        {
            get => _filterWorld;
            set => SetProperty(ref _filterWorld, value);
        }

        public string? FilterRouteName
        {
            get => _filterRouteName;
            set => SetProperty(ref _filterRouteName, value);
        }

        public string? FilterCreatorName
        {
            get => _filterCreatorName;
            set => SetProperty(ref _filterCreatorName, value);
        }

        public string? FilterZwiftRouteName
        {
            get => _filterZwiftRouteName;
            set => SetProperty(ref _filterZwiftRouteName, value);
        }

        public int? FilterDistanceMin
        {
            get => _filterDistanceMin;
            set => SetProperty(ref _filterDistanceMin, value);
        }

        public int? FilterDistanceMax
        {
            get => _filterDistanceMax;
            set => SetProperty(ref _filterDistanceMax, value);
        }

        public int? FilterAscentMin
        {
            get => _filterAscentMin;
            set => SetProperty(ref _filterAscentMin, value);
        }

        public int? FilterAscentMax
        {
            get => _filterAscentMax;
            set => SetProperty(ref _filterAscentMax, value);
        }

        public int? FilterDescentMin
        {
            get => _filterDescentMin;
            set => SetProperty(ref _filterDescentMin, value);
        }

        public int? FilterDescentMax
        {
            get => _filterDescentMax;
            set => SetProperty(ref _filterDescentMax, value);
        }

        public bool IsLoopYesChecked
        {
            get => _isLoopYesChecked;
            set
            {
                SetProperty(ref _isLoopYesChecked, value);
                OnPropertyChanged(nameof(FilterIsLoop));
            }
        }

        public bool IsLoopNoChecked
        {
            get => _isLoopNoChecked;
            set
            {
                SetProperty(ref _isLoopNoChecked, value);
                OnPropertyChanged(nameof(FilterIsLoop));
            }
        }

        public bool IsLoopBothChecked
        {
            get => _isLoopBothChecked;
            set
            {
                SetProperty(ref _isLoopBothChecked, value);
                OnPropertyChanged(nameof(FilterIsLoop));
            }
        }

        public bool? FilterIsLoop
        {
            get
            {
                if (IsLoopYesChecked)
                {
                    return true;
                }

                if (IsLoopNoChecked)
                {
                    return false;
                }

                return null;
            }
        }


        private async Task<CommandResult> LoadRoutesForRepositoryAsync(string repository)
        {
            try
            {
                var command = new SearchRouteCommand(
                    repository,
                    FilterWorld?.Id,
                    FilterCreatorName,
                    FilterRouteName,
                    FilterZwiftRouteName,
                    FilterDistanceMin == 0 ? null : FilterDistanceMin,
                    FilterDistanceMax == 0 ? null : FilterDistanceMax,
                    FilterAscentMin == 0 ? null : FilterAscentMin,
                    FilterAscentMax == 0 ? null : FilterAscentMax,
                    FilterDescentMin == 0 ? null : FilterDescentMin,
                    FilterDescentMax == 0 ? null : FilterDescentMax,
                    FilterIsLoop,
                    null,
                    null
                    );

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                Routes = (await _useCase.ExecuteAsync(command, tokenSource.Token))
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
