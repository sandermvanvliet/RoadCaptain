using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoadCaptain.Runner.Annotations;

namespace RoadCaptain.Runner.Models
{
    public class SegmentSequenceModel : INotifyPropertyChanged
    {
        private string _turnImage;
        private SegmentDirection _direction;
        private readonly double _ascent;
        private readonly double _descent;
        private TrackPoint _pointOnSegment;

        public SegmentSequenceModel(SegmentSequence segmentSequence, Segment segment, int sequenceNumber)
        {
            Model = segmentSequence;
            TurnImage = ImageFromTurn(segmentSequence.TurnToNextSegment);
            _ascent = Math.Round(segment.Ascent, 1);
            _descent = Math.Round(segment.Descent, 1);
            Distance = Math.Round(segment.Distance / 1000, 1);
            SequenceNumber = sequenceNumber + 1; // Indexes are zero based...
            Direction = segmentSequence.Direction;
        }

        private static string ImageFromTurn(TurnDirection turnDirection)
        {
            return turnDirection switch
            {
                TurnDirection.Left => "Assets/turnleft.png",
                TurnDirection.Right => "Assets/turnright.png",
                TurnDirection.GoStraight => "Assets/gostraight.png",
                _ => "Assets/finish.png"
            };
        }

        public SegmentSequence Model { get; }

        public int SequenceNumber { get; }

        public string TurnImage
        {
            get => _turnImage;
            private init
            {
                _turnImage = value;
                OnPropertyChanged();
            }
        }

        public string SegmentId => Model.SegmentId;
        public double Distance { get; }

        public TrackPoint PointOnSegment
        {
            get => _pointOnSegment;
            set
            {
                if (Equals(value, _pointOnSegment)) return;
                _pointOnSegment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DistanceOnSegment));
            }
        }

        public double DistanceOnSegment
        {
            get
            {
                if (PointOnSegment == null)
                {
                    return 0;
                }

                return Direction switch
                {
                    SegmentDirection.AtoB => PointOnSegment.DistanceOnSegment / 1000,
                    SegmentDirection.BtoA => Distance - (PointOnSegment.DistanceOnSegment / 1000),
                    _ => 0
                };
            }
        }

        public double Ascent
        {
            get
            {
                return Direction switch
                {
                    SegmentDirection.AtoB => _ascent,
                    SegmentDirection.BtoA => _descent,
                    _ => 0
                };
            }
        }

        public double Descent
        {
            get
            {
                return Direction switch
                {
                    SegmentDirection.AtoB => _descent,
                    SegmentDirection.BtoA => _ascent,
                    _ => 0
                };
            }
        }

        public SegmentDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Ascent));
                OnPropertyChanged(nameof(Descent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}