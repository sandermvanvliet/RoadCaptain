using ReactiveUI;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class AboutRoadCaptainViewModel : ViewModelBase
    {
        private string _version;

        public AboutRoadCaptainViewModel()
        {
            Version = GetType().Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                this.RaisePropertyChanged();
            }
        }
    }
}