using System.Collections.Generic;
using RoadCaptain.App.Runner.Models;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeInGameViewModel : InGameNavigationWindowViewModel
    {
        private static readonly List<Segment> DefaultSegments = new List<Segment>();

        public DesignTimeInGameViewModel() 
            : base(
                new InGameWindowModel(DefaultSegments), DefaultSegments)
        {
        }
    }
}