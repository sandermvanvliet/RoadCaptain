using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.Adapters;
using RoadCaptain.RouteBuilder.Annotations;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Segment _selectedSegment;

        public MainViewModel()
        {
            Route = new RouteViewModel();

            Segments = new SegmentStore().LoadSegments();
        }

        public List<Segment> Segments { get; }

        public RouteViewModel Route { get; set; }

        public Segment SelectedSegment
        {
            get => _selectedSegment;
            private set
            {
                _selectedSegment = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SelectSegment(string segmentId)
        {
            SelectedSegment = Segments.SingleOrDefault(s => s.Id == segmentId);
        }

        public void ClearSelectedSegment()
        {
            SelectedSegment = null;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}