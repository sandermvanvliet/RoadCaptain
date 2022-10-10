// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class SportViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isDefault;
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

        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                if (value == _isDefault)
                {
                    return;
                }

                _isDefault = value;
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
