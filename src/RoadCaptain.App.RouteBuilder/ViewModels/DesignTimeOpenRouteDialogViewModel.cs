// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeOpenRouteDialogViewModel : OpenRouteDialogViewModel
    {
        public DesignTimeOpenRouteDialogViewModel() : base(new DesignTimeWindowService(), new DummyUserPreferences(), null)
        {
        }
    }
}
