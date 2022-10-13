namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class MarkerViewModel : ViewModelBase
    {
        public MarkerViewModel(Segment marker)
        {
            Id = marker.Id;
            Name = marker.Name;
            Type = marker.Type;
        }

        public string Id { get; set; }
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