// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.App.Shared.ViewModels;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class LandingPageViewModel : ViewModelBase
    {
        private WorldViewModel[] _worlds = [];
        private SportViewModel[] _sports = [];
        private readonly IUserPreferences _userPreferences;
        private readonly IWindowService _windowService;
        private SportViewModel? _selectedSport;
        private WorldViewModel? _selectedWorld;
        private Shared.ViewModels.RouteViewModel[] _myRoutes = [];
        private Shared.ViewModels.RouteViewModel? _selectedRoute;
        private readonly SearchRoutesUseCase _searchRoutesUseCase;
        private bool _inProgress = true;
        private readonly LoadRouteFromFileUseCase _loadRouteFromFileUseCase;
        private readonly DeleteRouteUseCase _deleteRouteUseCase;
        private bool _canBuildRoute;

        public LandingPageViewModel(
            IWorldStore worldStore, 
            IUserPreferences userPreferences, 
            IWindowService windowService, 
            SearchRoutesUseCase searchRoutesUseCase, 
            LoadRouteFromFileUseCase loadRouteFromFileUseCase, 
            DeleteRouteUseCase deleteRouteUseCase)
        {
            _userPreferences = userPreferences;
            _windowService = windowService;
            _searchRoutesUseCase = searchRoutesUseCase;
            _loadRouteFromFileUseCase = loadRouteFromFileUseCase;
            _deleteRouteUseCase = deleteRouteUseCase;

            Worlds = worldStore.LoadWorlds().Select(world => new WorldViewModel(world)).ToArray();
            Sports = [new SportViewModel(SportType.Cycling, DefaultSport), new SportViewModel(SportType.Running, DefaultSport)];
            SelectedSport = Sports.SingleOrDefault(s => s.IsSelected);
            
            SelectWorldCommand = new AsyncRelayCommand(
                _ => SelectWorld(_ as WorldViewModel ??
                                 throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => (_ as WorldViewModel)?.CanSelect ?? false);

            SelectSportCommand = new AsyncRelayCommand(
                _ => SelectSport(_ as SportViewModel ??
                                 throw new ArgumentNullException(nameof(RelayCommand.CommandParameter))),
                _ => _ is SportViewModel);

            ResetDefaultSportCommand = new RelayCommand(
                _ => ResetDefaultSport(),
                _ => true);

            LoadMyRoutesCommand = new AsyncRelayCommand(
                async _ => await LoadMyRoutes(),
                _ => !InProgress)
                .SubscribeTo(this, () => InProgress);

            SearchRouteCommand = new AsyncRelayCommand(
                async _ => await SearchRoute(),
                _ => !InProgress)
                .SubscribeTo(this, () => InProgress);

            OpenRouteFromFileCommand = new AsyncRelayCommand(
                async _ => await OpenRouteFromFile(),
                _ => !InProgress)
                .SubscribeTo(this, () => InProgress);

            DeleteRouteCommand = new AsyncRelayCommand(
                async route => await DeleteRouteAsync(route as Shared.ViewModels.RouteViewModel),
                parameter => parameter is Shared.ViewModels.RouteViewModel routeViewModel &&
                             routeViewModel.Uri != null &&
                             !routeViewModel.IsReadOnly &&
                             !InProgress)
                .OnSuccess(async _ => await LoadMyRoutes())
                .OnFailure(async result => await _windowService.ShowErrorDialog(result.Message))
                .SubscribeTo(this, () => InProgress);
        }

        private async Task<CommandResult> DeleteRouteAsync(Shared.ViewModels.RouteViewModel? route)
        {
            if (route?.Uri == null)
            {
                return CommandResult.Failure("Route does not have an URI and I don't know where to go to delete it");
            }

            try
            {
                InProgress = true;

                await _deleteRouteUseCase.ExecuteAsync(new DeleteRouteCommand(route.Uri, route.RepositoryName));

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Failure(e.Message);
            }
            finally
            {
                InProgress = false;
            }
        }

        private async Task<CommandResult> OpenRouteFromFile()
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

            var plannedRoute = _loadRouteFromFileUseCase.Execute(new LoadFromFileCommand(filePath));

            SelectedRoute = new Shared.ViewModels.RouteViewModel(new RouteModel
            {
                PlannedRoute = plannedRoute,
                // We can safely set this because when a user is opening
                // files from their local machine they're allowed to edit them
                Uri = new Uri(filePath),
                RepositoryName = null
            });

            return CommandResult.Success();
        }

        private async Task<CommandResult> SearchRoute()
        {
            var result = await _windowService.ShowSelectRouteDialog();

            if (result == null)
            {
                return CommandResult.Aborted();
            }
            
            // TODO: Do something clever where we retain these values if the user actually owns this route...
            result.Uri = null;
            result.RepositoryName = null;
            
            SelectedRoute = new Shared.ViewModels.RouteViewModel(result);
            
            return CommandResult.Success();
        }

        private async Task<CommandResult> LoadMyRoutes()
        {
            InProgress = true;

            try
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                var currentUser = "Sander van Vliet [RoadCaptain]";

                var result = await _searchRoutesUseCase.ExecuteAsync(
                    new SearchRouteCommand(RetrieveRepositoriesIntent.Manage, creator: currentUser), tokenSource.Token);

                var theRoutes = result
                    .Select(r => new Shared.ViewModels.RouteViewModel(r))
                    .ToArray();
                MyRoutes = theRoutes;

                return CommandResult.Success();
            }
            catch (OperationCanceledException)
            {
                return CommandResult.Aborted();
            }
            finally
            {
                InProgress = false;
            }
        }

        public bool InProgress
        {
            get => _inProgress;
            protected set => SetProperty(ref _inProgress, value);
        }

        public ICommand SelectWorldCommand { get; }
        public ICommand SelectSportCommand { get; }
        public ICommand ResetDefaultSportCommand { get; }
        public ICommand LoadMyRoutesCommand { get; }
        public ICommand SearchRouteCommand { get; }
        public ICommand OpenRouteFromFileCommand { get; }
        public ICommand DeleteRouteCommand { get; }

        public WorldViewModel[] Worlds
        {
            get => _worlds;
            private set => SetProperty(ref _worlds, value);
        }

        public SportViewModel[] Sports
        {
            get => _sports;
            private set => SetProperty(ref _sports, value);
        }

        public WorldViewModel? SelectedWorld
        {
            get => _selectedWorld;
            private set
            {
                SetProperty(ref _selectedWorld, value);
                CanBuildRoute = SelectedWorld != null && SelectedSport != null;
            }
        }

        public SportViewModel? SelectedSport
        {
            get => _selectedSport;
            private set
            {
                SetProperty(ref _selectedSport, value);
                CanBuildRoute = SelectedWorld != null && SelectedSport != null;
            }
        }

        public bool CanBuildRoute
        {
            get => _canBuildRoute;
            set => SetProperty(ref _canBuildRoute, value);
        }

        public bool HasDefaultSport => !string.IsNullOrEmpty(DefaultSport);

        public string? DefaultSport
        {
            get => _userPreferences.DefaultSport;
            private set
            {
                _userPreferences.DefaultSport = value;
                _userPreferences.Save();

                OnPropertyChanged(nameof(DefaultSport));
                OnPropertyChanged(nameof(HasDefaultSport));
            }
        }

        public Shared.ViewModels.RouteViewModel[] MyRoutes
        {
            get => _myRoutes;
            set => SetProperty(ref _myRoutes, value.OrderBy(r => r.World).ThenBy(r => r.Name).ToArray());
        }

        public Shared.ViewModels.RouteViewModel? SelectedRoute
        {
            get => _selectedRoute;
            set => SetProperty(ref _selectedRoute, value);
        }

        private Task<CommandResult> SelectWorld(WorldViewModel world)
        {
            if (string.IsNullOrEmpty(world.Id))
            {
                return Task.FromResult<CommandResult>(CommandResult.Failure("Can't select the world because its id is empty"));
            }

            SelectedWorld = world;

            var currentSelected = Worlds.SingleOrDefault(w => w.IsSelected);
            if (currentSelected != null)
            {
                currentSelected.IsSelected = false;
            }

            world.IsSelected = true;

            return Task.FromResult(CommandResult.Success());
        }

        private async Task<CommandResult> SelectSport(SportViewModel sport)
        {
            SelectedSport = sport;

            if (string.IsNullOrEmpty(_userPreferences.DefaultSport))
            {
                var result = await _windowService.ShowDefaultSportSelectionDialog(sport.Sport);

                if (result)
                {
                    DefaultSport = sport.Sport.ToString();
                    sport.IsDefault = true;
                }
            }

            var currentSelected = Sports.SingleOrDefault(w => w.IsSelected);
            if (currentSelected != null)
            {
                currentSelected.IsSelected = false;
            }

            sport.IsSelected = true;

            return CommandResult.Success();
        }

        private CommandResult ResetDefaultSport()
        {
            _userPreferences.DefaultSport = null;
            _userPreferences.Save();

            foreach (var sport in _sports)
            {
                sport.IsDefault = false;
            }

            DefaultSport = null;
            SelectedSport = null;

            return CommandResult.Success();
        }
    }
}
