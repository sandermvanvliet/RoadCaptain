// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using ReactiveUI;
using RoadCaptain.App.Shared.ViewModels;


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
            Segment = segment;
        }

        private static string GlyphFromTurn(TurnDirection turnDirection)
        {
            return turnDirection switch
            {
                TurnDirection.Left => "⬅",
                TurnDirection.Right => "⮕",
                TurnDirection.GoStraight => "⬆",
                _ => "🏁"
            };
        }

        public SegmentSequence Model { get; }
        public Segment Segment { get; }

        public int SequenceNumber { get; }

        public string? TurnGlyph
        {
            get => _turnGlyph;
            private set => SetProperty(ref _turnGlyph, value);
        }

        public string? SegmentId => Model.SegmentId;
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(Ascent));
                OnPropertyChanged(nameof(Descent));
            }
        }

        public string SegmentName { get; }

        public string? NoSelectReason { get; }

        public SegmentSequenceType Type
        {
            get => Model.Type;
            set
            {
                if (Model.Type == value) return;
                Model.Type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLeadIn));
                OnPropertyChanged(nameof(IsLoop));
                OnPropertyChanged(nameof(ColumnSpan));
                OnPropertyChanged(nameof(LoopImage));
            }
        }

        public bool IsLoop => Model.Type is SegmentSequenceType.Loop or SegmentSequenceType.LoopStart or SegmentSequenceType.LoopEnd;
        public bool IsLeadIn => Model.Type is SegmentSequenceType.LeadIn or SegmentSequenceType.LeadOut;
        public int ColumnSpan => IsLoop || IsLeadIn ? 1 : 2;

        public string? LoopImage
        {
            get
            {
                if (Model.Type == SegmentSequenceType.LoopStart)
                {
                    return "avares://RoadCaptain.App.RouteBuilder/Assets/loop-start.png";
                }

                if (Model.Type == SegmentSequenceType.LoopEnd)
                {
                        return "avares://RoadCaptain.App.RouteBuilder/Assets/loop-end.png";
                }

                if(Model.Type == SegmentSequenceType.Loop)
                {
                    return "avares://RoadCaptain.App.RouteBuilder/Assets/loop-middle.png";
                }

                if (Model.Type == SegmentSequenceType.LeadIn || 
                    Model.Type == SegmentSequenceType.LeadOut)
                {
                    return "avares://RoadCaptain.App.RouteBuilder/Assets/lead-in.png";
                }

                return null;
            }
        }

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
