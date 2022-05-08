using System;
using ReactiveUI;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class SegmentSequenceViewModel : ViewModelBase
    {
        private SegmentDirection _direction;
        private readonly double _ascent;
        private readonly double _descent;
        private string? _turnGlyph;

        public SegmentSequenceViewModel(SegmentSequence segmentSequence, Segment segment, int sequenceNumber)
        {
            Model = segmentSequence;
            TurnGlyph = GlyphFromTurn(segmentSequence.TurnToNextSegment);
            _ascent = Math.Round(segment.Ascent, 1);
            _descent = Math.Round(segment.Descent, 1);
            Distance = Math.Round(segment.Distance / 1000, 1);
            SequenceNumber = sequenceNumber;
            Direction = SegmentDirection.Unknown;
            SegmentName = segment.Name;
            NoSelectReason = segment.NoSelectReason;
        }

        private static string? GlyphFromTurn(TurnDirection turnDirection)
        {
            return turnDirection switch
            {
                TurnDirection.Left => "🡸",
                TurnDirection.Right => "🡺",
                TurnDirection.GoStraight => "🡹",
                _ => "🏁"
            };
        }

        public SegmentSequence Model { get; }

        public int SequenceNumber { get; }

        public string? TurnGlyph
        {
            get => _turnGlyph;
            private set
            {
                _turnGlyph = value;
                this.RaisePropertyChanged(nameof(TurnGlyph));
            }
        }

        public string SegmentId => Model.SegmentId;
        public double Distance { get; }

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
                this.RaisePropertyChanged(nameof(Direction));
                this.RaisePropertyChanged(nameof(Ascent));
                this.RaisePropertyChanged(nameof(Descent));
            }
        }

        public string SegmentName { get; }

        public string NoSelectReason { get; }

        public void SetTurn(TurnDirection direction, string ontoSegmentId, SegmentDirection segmentDirection)
        {
            Model.TurnToNextSegment = direction;
            Model.NextSegmentId = ontoSegmentId;
            Model.Direction = segmentDirection;

            TurnGlyph = GlyphFromTurn(direction);
            Direction = segmentDirection;
        }

        public void ResetTurn()
        {
            Model.TurnToNextSegment = TurnDirection.None;
            Model.NextSegmentId = null;

            TurnGlyph = null;
        }
    }
}