using System.Diagnostics;
using Codenizer.Avalonia.Map;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class MapSegment : MapObject
    {
        private readonly SKPath _path;
        private bool _isOnRoute;
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsLeadIn { get; set; }
        public bool IsLeadOut { get; set; }
        public bool IsLoop { get; set; }

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
            }
        }

        public MapSegment(string segmentId, SKPoint[] points)
        {
            _path = new SKPath();
            _path.AddPoly(points, false);

            SegmentId = segmentId;
            Name = $"segment-{segmentId}";
            Bounds = _path.TightBounds;
        }

        public override string Name { get; }
        public override SKRect Bounds { get; }
        public string SegmentId { get; }
        public SKPoint[] Points => _path.Points;

        public override void Render(SKCanvas canvas)
        {
            var currentPaint = SkiaPaints.SegmentPathPaint;

            if (IsLeadIn || IsLeadOut)
            {
                currentPaint = SkiaPaints.LeadInPaint;
            }
            else if (IsLoop)
            {
                currentPaint = SkiaPaints.LoopPaint;
            }
            else if (IsSelected)
            {
                currentPaint = SkiaPaints.SelectedSegmentPathPaint;
            }
            else if (IsHighlighted)
            {
                currentPaint = SkiaPaints.SegmentHighlightPaint;
            }
            else if (IsOnRoute)
            {
                currentPaint = SkiaPaints.RoutePathPaint;
            }

            canvas.DrawPath(_path, currentPaint);
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}