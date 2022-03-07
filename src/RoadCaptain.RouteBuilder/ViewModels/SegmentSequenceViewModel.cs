using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoadCaptain.RouteBuilder.Annotations;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class SegmentSequenceViewModel : INotifyPropertyChanged
    {
        private string _turnImage;

        public SegmentSequenceViewModel(SegmentSequence segmentSequence)
        {
            Model = segmentSequence;
            TurnImage = ImageFromTurn(segmentSequence.TurnToNextSegment);
        }

        private static string ImageFromTurn(TurnDirection turnDirection)
        {
            switch (turnDirection)
            {
                case TurnDirection.Left:
                    return "turnleft.jpg";
                case TurnDirection.Right:
                    return "turnright.jpg";
                default:
                    return "gostraight.jpg";
            }
        }

        public SegmentSequence Model { get; }

        public int SequenceNumber { get; }

        public string TurnImage
        {
            get => _turnImage;
            private set
            {
                _turnImage = value;
                OnPropertyChanged();
            }
        }

        public string SegmentId => Model.SegmentId;
        public double Distance { get; }
        public double Descent { get; }
        public double Ascent { get; }

        public void SetTurn(TurnDirection direction, string ontoSegmentId)
        {
            Model.TurnToNextSegment = direction;
            Model.NextSegmentId = ontoSegmentId;

            TurnImage = ImageFromTurn(direction);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}