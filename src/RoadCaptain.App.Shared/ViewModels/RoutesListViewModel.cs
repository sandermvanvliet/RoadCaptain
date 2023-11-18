﻿using ReactiveUI;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Shared.ViewModels
{
    public class RoutesListViewModel : ViewModelBase
    {
        private RouteViewModel[] _routes;

        public RouteViewModel[] Routes
        {
            get => _routes;
            set
            {
                if (value == _routes)
                {
                    return;
                }

                _routes = value;

                this.RaisePropertyChanged();
            }
        }
    }
}