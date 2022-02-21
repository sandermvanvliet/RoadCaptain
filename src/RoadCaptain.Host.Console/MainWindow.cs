using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoadCaptain.Host.Console.HostedServices;
using RoadCaptain.Ports;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RoadCaptain.Host.Console
{
    public partial class MainWindow : Form
    {
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly ISynchronizer _synchronizer;
        private SKPath _riderPath = new();

        private readonly SKPaint _riderPathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 2 };

        private readonly SKPaint _segmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        private readonly SKPaint _riderPositionPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };
        
        private readonly List<SKPath> _segmentPaths = new();
        private readonly ISegmentStore _segmentStore;
        private readonly CancellationTokenSource _tokenSource = new();
        private Offsets _overallOffsets;
        private TrackPoint _previousRiderPosition;
        private Task _receiverTask;
        private List<Segment> _segments;
        private bool _isInitialized;

        public MainWindow(
            ISegmentStore segmentStore,
            IGameStateReceiver gameStateReceiver,
            ISynchronizer synchronizer)
        {
            _segmentStore = segmentStore;
            _gameStateReceiver = gameStateReceiver;
            _synchronizer = synchronizer;

            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Load segments
            _segments = _segmentStore.LoadSegments();

            // Paint segments
            CreatePathsForSegments();
            
            // Only register callbacks after the form is initialized
            // otherwise we may get callback invocation before we're
            // ready to handle them.
            _gameStateReceiver.Register(
                UpdatePosition,
                UpdateCurrentSegemnt,
                UpdateAvailableTurns,
                UpdateDirection,
                UpdateTurnCommands,
                EnteredGame,
                LeftGame, 
                RouteSelected,
                null);

            // This starts the receiver if that has not yet been
            // done by another consumer.
            _receiverTask = Task.Factory.StartNew(() => { _gameStateReceiver.Start(_tokenSource.Token); });

            _isInitialized = true;
        }

        private void RouteSelected(PlannedRoute route)
        {
            dataGridViewRoute.Invoke(() => PopulateListBox(route));
        }

        private void PopulateListBox(PlannedRoute route)
        {
            dataGridViewRoute.DataSource = route
                .RouteSegmentSequence
                .Select((segment, index) => new RouteItem
                {
                    Step = index + 1,
                    Segment = segment.SegmentId,
                    Direction = segment.TurnToNextSegment == TurnDirection.Left ? "🡄" : 
                        segment.TurnToNextSegment == TurnDirection.Right
                        ? "🡆"
                        : "🡅"
                })
                .ToList();
        }

        private void CreatePathsForSegments()
        {
            _segmentPaths.Clear();

            var segmentsWithOffsets = _segments
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point =>
                        TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude)).ToList()
                })
                .Select(x => new
                {
                    x.Segment,
                    x.GameCoordinates,
                    Offsets = new Offsets(skControl1.Width, x.GameCoordinates)
                })
                .ToList();

            _overallOffsets = new Offsets(
                skControl1.Width,
                segmentsWithOffsets.SelectMany(s => s.GameCoordinates).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                _segmentPaths.Add(SkiaPathFromSegment(_overallOffsets, segment.GameCoordinates));
            }
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<TrackPoint> data)
        {
            var path = new SKPath();

            path.AddPoly(
                data
                    .Select(point => ScaleAndTranslate(point, offsets))
                    .Select(point => new SKPoint(point.X, point.Y))
                    .ToArray(),
                false);

            return path;
        }

        private static PointF ScaleAndTranslate(TrackPoint point, Offsets offsets)
        {
            var translatedX = offsets.OffsetX + (float)point.Latitude;
            var translatedY = offsets.OffsetY + (float)point.Longitude;

            var scaledX = translatedX * offsets.ScaleFactor;
            var scaledY = translatedY * offsets.ScaleFactor;

            return new PointF(scaledX, scaledY);
        }

        private void UpdateAvailableTurns(List<Turn> turns)
        {
            var text = string.Join(
                ", ",
                turns.Select(t => $"{t.Direction} => {t.SegmentId}"));

            textBoxAvailableTurns.Invoke((Action)(() => textBoxAvailableTurns.Text = text));
        }

        private void UpdateCurrentSegemnt(string segmentId)
        {
            var text = string.Empty;

            var segment = _segments.SingleOrDefault(s => s.Id == segmentId);

            if (segment != null)
            {
                text = segment.Id;
            }

            textBoxCurrentSegment.Invoke((Action)(() => textBoxCurrentSegment.Text = text));
        }

        private void UpdatePosition(TrackPoint point)
        {
            var currentPoint = TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude);

            if (_previousRiderPosition != null)
            {
                var scaledAndTranslated = ScaleAndTranslate(currentPoint, _overallOffsets);
                var scaledAndTranslatedPrevious = ScaleAndTranslate(_previousRiderPosition, _overallOffsets);

                _riderPath.AddPoly(
                    new[]
                    {
                        new SKPoint(scaledAndTranslatedPrevious.X, scaledAndTranslatedPrevious.Y),
                        new SKPoint(scaledAndTranslated.X, scaledAndTranslated.Y)
                    },
                    false);
            }

            _previousRiderPosition = currentPoint;

            // Force redraw of the canvas
            skControl1.Invalidate();
        }

        private void UpdateTurnCommands(List<TurnDirection> commands)
        {
            var text = string.Join(
                ", ",
                commands.Select(t => t.ToString()));

            textBoxAvailableCommands.Invoke((Action)(() => textBoxAvailableCommands.Text = text));

            SetPictureBoxVisibility(pictureBoxTurnLeft, commands.Any(c => c == TurnDirection.Left));
            SetPictureBoxVisibility(pictureBoxTurnRight, commands.Any(c => c == TurnDirection.Right));
            SetPictureBoxVisibility(pictureBoxGoStraight, commands.Any(c => c == TurnDirection.GoStraight));
        }

        private static void SetPictureBoxVisibility(PictureBox pictureBox, bool isVisible)
        {
            pictureBox.Invoke(() => pictureBox.Visible = isVisible);
        }

        private void UpdateDirection(SegmentDirection direction)
        {
            textBoxCurrentDirection.Invoke((Action)(() => textBoxCurrentDirection.Text = direction.ToString()));
        }

        private void LeftGame(ulong activityId)
        {
            
        }

        private void EnteredGame(ulong activityId)
        {
            _previousRiderPosition = null;
            _riderPath = new SKPath();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _tokenSource.Cancel();

            try
            {
                _receiverTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void skControl1_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();

            foreach (var skPath in _segmentPaths)
            {
                args.Surface.Canvas.DrawPath(skPath, _segmentPathPaint);
            }

            args.Surface.Canvas.DrawPath(_riderPath, _riderPathPaint);

            if (_previousRiderPosition != null)
            {
                // At this point the previous position is already the current position. See UpdatePosition()
                var scaledAndTranslated = ScaleAndTranslate(_previousRiderPosition, _overallOffsets);
                const int radius = 15;
                args.Surface.Canvas.DrawCircle(scaledAndTranslated.X, scaledAndTranslated.Y, radius, _riderPositionPaint);
            }

            args.Surface.Canvas.Flush();
        }

        private void skControl1_SizeChanged(object sender, EventArgs e)
        {
            if (!_isInitialized)
            {
                return;
            }

            CreatePathsForSegments();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            _synchronizer.TriggerSynchronizationEvent();
        }
    }

    internal class RouteItem
    {
        public int Step { get; set; }
        public string Segment { get; set; }
        public string Direction { get; set; }
    }
}