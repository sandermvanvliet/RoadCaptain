// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
