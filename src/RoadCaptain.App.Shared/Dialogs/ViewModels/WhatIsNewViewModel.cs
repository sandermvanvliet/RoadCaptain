// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using ReactiveUI;

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
            set
            {
                if (value == _version) return;
                _version = value;
                this.RaisePropertyChanged();
            }
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            set
            {
                if (value == _releaseNotes) return;
                _releaseNotes = value;
                this.RaisePropertyChanged();
            }
        }
    }
}
