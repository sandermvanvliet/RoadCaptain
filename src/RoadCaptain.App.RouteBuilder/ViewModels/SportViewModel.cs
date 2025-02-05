// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using RoadCaptain.App.Shared.ViewModels;


namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class SportViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isDefault;
        public SportType Sport { get; }

        public SportViewModel(SportType sport, string? defaultSport)
        {
            Sport = sport;
            
            if (sport.ToString() == defaultSport)
            {
                IsSelected = true;
                IsDefault = true;
            }
        }
        
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public DrawingImage? Image {
            get
            {
                if (Application.Current!.TryFindResource($"{Sport}DrawingImage", out var resource))
                {
                    return resource as DrawingImage;
                }

                return null;
            }
        }
    }
}
