using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RoadCaptain.RouteBuilder.Annotations;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class RouteViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<SegmentSequenceViewModel> _sequence = new();

        public IEnumerable<SegmentSequenceViewModel> Sequence => _sequence;

        public double TotalDistance => Sequence.Sum(s => s.Distance);
        public double TotalAscent => Sequence.Sum(s => s.Ascent);
        public double TotalDescent => Sequence.Sum(s => s.Descent);

        public SegmentSequenceViewModel Last => Sequence.LastOrDefault();

        public void StartOn(Segment segment)
        {
            _sequence.Add(new SegmentSequenceViewModel(new SegmentSequence
            {
                SegmentId = segment.Id,
                TurnToNextSegment = TurnDirection.None,
                NextSegmentId = null
            }));

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public void NextStep(TurnDirection direction, string ontoSegmentId)
        {
            Last.SetTurn(direction, ontoSegmentId);
            
            _sequence.Add(new SegmentSequenceViewModel(new SegmentSequence
            {
                SegmentId = ontoSegmentId,
                TurnToNextSegment = TurnDirection.None,
                NextSegmentId = null
            }));

            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(TotalDistance));
            OnPropertyChanged(nameof(TotalAscent));
            OnPropertyChanged(nameof(TotalDescent));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}