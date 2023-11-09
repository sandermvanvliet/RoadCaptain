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
            set
            {
                if (value == _worlds) return;
                _worlds = value;
                this.RaisePropertyChanged();
            }
        }

        public SportViewModel[] Sports
        {
            get => _sports;
            set
            {
                if (value == _sports) return;
                _sports = value;
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

        private async Task<CommandResult> SelectWorld(WorldViewModel world)
        {
            if (string.IsNullOrEmpty(world.Id))
            {
                return CommandResult.Failure("Can't select the world because its id is empty");
            }

            // TODO: fixme
            // Route.World = _worldStore.LoadWorldById(world.Id);

            var currentSelected = Worlds.SingleOrDefault(w => w.IsSelected);
            if (currentSelected != null)
            {
                currentSelected.IsSelected = false;
            }

            world.IsSelected = true;

            // TODO: fixme
            // if (Route.ReadyToBuild)
            // {
            //     this.RaisePropertyChanged(nameof(Route));
            // }

            return CommandResult.Success();
        }

        private async Task<CommandResult> SelectSport(SportViewModel sport)
        {
            // TODO: fixme
            // Route.Sport = sport.Sport;

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

            // TODO: fixme
            // if (Route.ReadyToBuild)
            // {
            //     this.RaisePropertyChanged(nameof(Route));
            // }

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

            return CommandResult.Success();
        }
    }
}