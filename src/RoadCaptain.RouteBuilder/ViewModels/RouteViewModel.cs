using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class RouteViewModel
    {
        public IEnumerable<SegmentSequenceViewModel> Sequence { get; set; } = new List<SegmentSequenceViewModel>();

        public double TotalDistance => Sequence.Sum(s => s.Distance);
        public double TotalAscent => Sequence.Sum(s => s.Ascent);
        public double TotalDescent => Sequence.Sum(s => s.Descent);
    }
}