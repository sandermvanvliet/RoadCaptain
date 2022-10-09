using ReactiveUI;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class WhatIsNewViewModel : ViewModelBase
    {
        private string _version;
        private string _releaseNotes;

        public WhatIsNewViewModel(Release release)
        {
            _version = release.Version.ToString(4);
            _releaseNotes = release.ReleaseNotes;
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