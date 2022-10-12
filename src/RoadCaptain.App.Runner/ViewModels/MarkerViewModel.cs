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

        public string TypeGlyph
        {
            get
            {
                return Type switch
                {
                    SegmentType.Climb => "⛰",
                    SegmentType.Sprint => "⏱",
                    SegmentType.StravaSegment => "S",
                    _ => ""
                };
            }
        }
    }
}