// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using ReactiveUI;
using RoadCaptain.App.Shared.ViewModels;


namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class WhatIsNewViewModel : ViewModelBase
    {
        private string _version;
        private string _releaseNotes;

        public WhatIsNewViewModel(Release release)
        {
            _version = (release.Version ?? new Version()).ToString(4);
            _releaseNotes = release.ReleaseNotes ?? string.Empty;
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            set => SetProperty(ref _releaseNotes, value);
        }
    }
}
