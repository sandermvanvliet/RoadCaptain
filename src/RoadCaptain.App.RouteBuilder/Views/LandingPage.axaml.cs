// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia.Controls;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class LandingPage : UserControl
    {
        public LandingPage()
        {
            InitializeComponent();
        }

        private void RoutesList_OnRouteSelected(object? sender, RouteSelectedEventArgs e)
        {
            ViewModel.SelectedRoute = e.Route;
        }

        private LandingPageViewModel ViewModel => (LandingPageViewModel)DataContext!;
    }
}
