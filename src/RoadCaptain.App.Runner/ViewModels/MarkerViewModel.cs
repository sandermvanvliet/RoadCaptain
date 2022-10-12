namespace RoadCaptain.App.Runner.ViewModels
{
    public class MarkerViewModel : ViewModelBase
    {
        public MarkerViewModel(Segment marker)
        {
            Name = marker.Name;
            Type = marker.Type;
        }

        public string Name { get; set; }
        public SegmentType Type { get; set; }
    }
}