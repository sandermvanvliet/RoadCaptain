// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace RoadCaptain.App.Runner.Models
{
    public class SegmentSequenceModel : INotifyPropertyChanged
    {
        private SegmentDirection _direction;
        private readonly double _ascent;
        private readonly double _descent;
        private TrackPoint? _pointOnSegment;

        public SegmentSequenceModel(SegmentSequence segmentSequence, Segment segment)
        {
            Model = segmentSequence;
            TurnGlyph = GlyphFromTurn(segmentSequence.TurnToNextSegment);
            _ascent = Math.Round(segment.Ascent, 1);
            _descent = Math.Round(segment.Descent, 1);
            Distance = Math.Round(segment.Distance / 1000, 1);
            SequenceNumber = segmentSequence.Index + 1; // Indexes are zero based...
            Direction = segmentSequence.Direction;
            SegmentName = segment.Name;
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

        public int SequenceNumber { get; }

        public string TurnGlyph { get; }

        public string SegmentId => Model.SegmentId;
        public double Distance { get; }

        public TrackPoint? PointOnSegment
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

        public string SegmentName { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
