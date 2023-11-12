using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using RoadCaptain.App.Shared.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class LandingPageViewModel : ViewModelBase
    {
        private WorldViewModel[] _worlds;
        private SportViewModel[] _sports;
        private readonly IUserPreferences _userPreferences;
        private readonly IWindowService _windowService;
        private SportViewModel? _selectedSport;
        private WorldViewModel? _selectedWorld;

        public LandingPageViewModel(IWorldStore worldStore, IUserPreferences userPreferences, IWindowService windowService)
        {
            _userPreferences = userPreferences;
            _windowService = windowService;

            _worlds = worldStore.LoadWorlds().Select(world => new WorldViewModel(world)).ToArray();
            _sports = new[] { new SportViewModel(SportType.Cycling), new SportViewModel(SportType.Running) };
            
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
        }
        
        public ICommand SelectWorldCommand { get; }
        public ICommand SelectSportCommand { get; }
        public ICommand ResetDefaultSportCommand { get; }

        public WorldViewModel[] Worlds
        {
            get => _worlds;
            private set
            {
                if (value == _worlds) return;
                _worlds = value;
                this.RaisePropertyChanged();
            }
        }

        public SportViewModel[] Sports
        {
            get => _sports;
            private set
            {
                if (value == _sports) return;
                _sports = value;
                this.RaisePropertyChanged();
            }
        }

        public WorldViewModel? SelectedWorld
        {
            get => _selectedWorld;
            private set
            {
                if (value == _selectedWorld) return;
                
                _selectedWorld = value;
                this.RaisePropertyChanged();
            }
        }

        public SportViewModel? SelectedSport
        {
            get => _selectedSport;
            private set
            {
                if (value == _selectedSport) return;
                
                _selectedSport = value;
                this.RaisePropertyChanged();
            }
        }

        public bool HasDefaultSport => !string.IsNullOrEmpty(DefaultSport);

        public string? DefaultSport
        {
            get => _userPreferences.DefaultSport;
            private set
            {
                _userPreferences.DefaultSport = value;
                _userPreferences.Save();

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasDefaultSport));
            }
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