using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoadCaptain.Runner.Annotations;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Model = new MainWindowModel
            {
                CurrentSegment = new SegmentSequenceModel(
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        NextSegmentId = "watopia-beach-island-loop-001-before",
                        SegmentId = "watopia-big-loop-001-after-after",
                        TurnToNextSegment = TurnDirection.Left
                    },
                    new Segment(new List<TrackPoint>()),
                    1),
                NextSegment = new SegmentSequenceModel(
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        NextSegmentId = null,
                        SegmentId = "watopia-beach-island-loop-001-before",
                        TurnToNextSegment = TurnDirection.None
                    },
                    new Segment(new List<TrackPoint>()),
                    2),
                Route = new PlannedRoute(),
                CurrentSegmentSequenceNumber = 1,
                ElapsedAscent = 12,
                ElapsedDescent = 0,
                ElapsedDistance = 5.1,
                RouteSegmentCount = 2
            };
        }

        public MainWindowModel Model { get; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}