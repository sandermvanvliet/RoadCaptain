using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class SportViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        public SportType Sport { get; }

        public SportViewModel(SportType sport)
        {
            Sport = sport;
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

        public DrawingImage Image {
            get
            {
                var resource = Application.Current.MainWindow.TryFindResource($"{Sport}DrawingImage");
                return resource as DrawingImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UserInterface.Shared.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}