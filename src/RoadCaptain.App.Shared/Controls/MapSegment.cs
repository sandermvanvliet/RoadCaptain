// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using Avalonia;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class MapSegment : MapObject
    {
        private readonly SKPath _path;
        private SKPaint _currentPaint;
        private bool _isHighlighted;
        private bool _isLeadIn;
        private bool _isLeadOut;
        private bool _isLoop;
        private bool _isOnRoute;
        private bool _isSelected;

        public MapSegment(string segmentId, SKPoint[] points)
        {
            _currentPaint = SkiaPaints.SegmentPathPaint;
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"segment-{segmentId}";
            Bounds = _path.TightBounds;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                DeterminePathPaint();
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                DeterminePathPaint();
            }
        }

        public bool IsLeadIn
        {
            get => _isLeadIn;
            set
            {
                _isLeadIn = value;
                DeterminePathPaint();
            }
        }

        public bool IsLeadOut
        {
            get => _isLeadOut;
            set
            {
                _isLeadOut = value;
                DeterminePathPaint();
            }
        }

        public bool IsLoop
        {
            get => _isLoop;
            set
            {
                _isLoop = value;
                DeterminePathPaint();
            }
        }

        public bool IsOnRoute
        {
            get => _isOnRoute;
            set
            {
                _isOnRoute = value;

                if (!_isOnRoute)
                {
                    IsLeadIn = false;
                    IsLeadOut = false;
                    IsLoop = false;
                }

                DeterminePathPaint();
            }
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public override bool IsSelectable { get; set; } = true;
        public override bool IsVisible { get; set; } = true;
        public string SegmentId { get; }
        public SKPoint[] Points => _path.Points;

        protected override void RenderCore(SKCanvas canvas)
        {
            canvas.DrawPath(_path, _currentPaint);
        }

        public override bool TightContains(SKPoint mapPosition)
        {
            return Points.Any(p => DistanceTo(p, mapPosition).Length < 10);
        }

        private static Vector DistanceTo(SKPoint a, SKPoint b)
        {
            return new Vector(
                Math.Abs(a.X - b.X),
                Math.Abs(a.Y - b.Y));
        }

        private void DeterminePathPaint()
        {
            if (IsLeadIn || IsLeadOut)
            {
                _currentPaint = SkiaPaints.LeadInPaint;
            }
            else if (IsLoop)
            {
                _currentPaint = SkiaPaints.LoopPaint;
            }
            else if (IsSelected)
            {
                _currentPaint = SkiaPaints.SelectedSegmentPathPaint;
            }
            else if (IsHighlighted)
            {
                _currentPaint = SkiaPaints.SegmentHighlightPaint;
            }
            else if (IsOnRoute)
            {
                _currentPaint = SkiaPaints.RoutePathPaint;
            }
            else
            {
                _currentPaint = SkiaPaints.SegmentPathPaint;
            }
        }
    }
}
