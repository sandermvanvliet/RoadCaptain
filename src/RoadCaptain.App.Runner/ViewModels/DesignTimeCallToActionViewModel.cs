// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeCallToActionViewModel : CallToActionViewModel
    {
        public DesignTimeCallToActionViewModel()
            : base(
                "Waiting for connection", 
                "Start Zwift and start cycling in Watopia on route The Mega Pretzel")
        {
        }
    }
}
