using ReactiveUI;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class WorldViewModel : ViewModelBase
    {
        private readonly World _world;
        private bool _isSelected;

        public WorldViewModel(World world)
        {
            _world = world;
        }


        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected)
                {
                    return;
                }

                _isSelected = value;
                this.RaisePropertyChanged(nameof(IsSelected));
            }
        }

        public string Image
        {
            get
            {
                return $"avares://RoadCaptain.App.Shared/Assets/world-{_world.Id.ToLower()}.jpg";
            }
        }

        public string Name => _world.Name;
        public bool CanSelect => _world.Status == WorldStatus.Available || _world.Status == WorldStatus.Beta;
        public string Id => _world.Id;
        public bool IsBeta => _world.Status == WorldStatus.Beta;
    }
}