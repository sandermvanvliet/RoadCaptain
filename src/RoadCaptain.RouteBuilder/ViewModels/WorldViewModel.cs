using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class WorldViewModel : INotifyPropertyChanged
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
                OnPropertyChanged();
            }
        }

        public string Image =>
            $"pack://application:,,,/RoadCaptain.UserInterface.Shared;component/Assets/world-{_world.Id}.jpg";

        public string Name => _world.Name;
        public bool CanSelect => _world.Status == WorldStatus.Available;
        public string Id => _world.Id;

        public event PropertyChangedEventHandler PropertyChanged;

        [UserInterface.Shared.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}