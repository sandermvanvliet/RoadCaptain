// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using RoadCaptain.App.Shared.ViewModels;


namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class WorldViewModel : ViewModelBase
    {
        private readonly World _world;
        private bool _isSelected;

        public WorldViewModel(World world)
        {
            if (string.IsNullOrEmpty(world.Id))
            {
                throw new ArgumentException("World id is empty");
            }

            _world = world;
        }


        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Image => $"avares://RoadCaptain.App.Shared/Assets/world-{_world.Id!.ToLower()}.jpg";
        public string Name => _world.Name ?? $"World {Id}";
        public bool CanSelect => _world.Status == WorldStatus.Available || _world.Status == WorldStatus.Beta;
        public string Id => _world.Id!;
        public bool IsBeta => _world.Status == WorldStatus.Beta;
    }
}
