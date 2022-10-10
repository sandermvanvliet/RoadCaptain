// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class SportViewModel : ViewModelBase
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
                this.RaisePropertyChanged(nameof(IsSelected));
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
                this.RaisePropertyChanged(nameof(IsDefault));
            }
        }

        public DrawingImage? Image {
            get
            {
                if (Application.Current.TryFindResource($"{Sport}DrawingImage", out var resource))
                {
                    return resource as DrawingImage;
                }

                return null;
            }
        }
    }
}
