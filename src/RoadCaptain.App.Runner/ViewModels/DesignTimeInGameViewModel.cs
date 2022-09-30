using System.Collections.Generic;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.GameStates;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeInGameViewModel : InGameNavigationWindowViewModel
    {
        private static readonly List<Segment> DefaultSegments = new List<Segment>();

        public DesignTimeInGameViewModel() 
            : base(
                new InGameWindowModel(DefaultSegments), DefaultSegments, null)
        {
            UpdateGameState(new OnSegmentState(1, 2, TrackPoint.Unknown, new Segment(new List<TrackPoint>()), SegmentDirection.AtoB, 0, 0, 0));
        }
    }
}