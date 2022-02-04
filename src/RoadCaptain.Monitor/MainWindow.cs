using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoadCaptain.Ports;

namespace RoadCaptain.Monitor
{
    public partial class MainWindow : Form
    {
        private readonly ISegmentStore _segmentStore;
        private readonly IGameStateReceiver _gameStateReceiver;
        private List<Segment> _segments;
        private readonly CancellationTokenSource _tokenSource = new();
        private Task _receiverTask;
        private TrackPoint _previousPoint;
        private Offsets _overallOffsets;

        public MainWindow(
            ISegmentStore segmentStore,
            IGameStateReceiver gameStateReceiver)
        {
            _segmentStore = segmentStore;
            _gameStateReceiver = gameStateReceiver;

            _gameStateReceiver.Register(
                UpdatePosition,
                UpdateCurrentSegemnt,
                UpdateAvailableTurns,
                UpdateDirection,
                UpdateTurnCommands);

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
            DrawSegments();

            _receiverTask = Task.Factory.StartNew(() =>
            {
                _gameStateReceiver.Start(_tokenSource.Token);
            });
        }

        private void DrawSegments()
        {
            var image = new Bitmap(pictureBoxMap.Width, pictureBoxMap.Height);

            using var graphics = Graphics.FromImage(image);

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
                    Offsets = new Offsets(pictureBoxMap.Width, x.GameCoordinates)
                })
                .ToList();

            _overallOffsets = new Offsets(
                pictureBoxMap.Width,
                segmentsWithOffsets.SelectMany(s => s.GameCoordinates).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                DrawSegment(_overallOffsets, segment.GameCoordinates, graphics);
            }

            pictureBoxMap.Image = image;
        }

        private void DrawSegment(Offsets offsets, List<TrackPoint> data, Graphics graphics)
        {
            for (var index = 1; index < data.Count; index++)
            {
                var previousPoint = data[index-1];
                var point = data[index];
                
                DrawSegmentLine(offsets, graphics, point, previousPoint, Pens.Red);
            }
        }

        private static void DrawSegmentLine(Offsets offsets, Graphics graphics, TrackPoint point, TrackPoint previousPoint, Pen pen)
        {
            var translatedX = (offsets.OffsetX + (float)point.Latitude);
            var translatedY = (offsets.OffsetY + (float)point.Longitude);

            var scaledX = (translatedX * offsets.ScaleFactor);
            var scaledY = (translatedY * offsets.ScaleFactor);

            var previousTranslatedX = (offsets.OffsetX + (float)previousPoint.Latitude);
            var previousTranslatedY = (offsets.OffsetY + (float)previousPoint.Longitude);
            var previousScaledX = (previousTranslatedX * offsets.ScaleFactor);
            var previousScaledY = (previousTranslatedY * offsets.ScaleFactor);

            try
            {
                graphics.DrawLine(pen, (int)previousScaledX, (int)previousScaledY, (int)scaledX, (int)scaledY);
            }
            catch (ArgumentOutOfRangeException)
            {
                Debugger.Break();
            }
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
            pictureBoxMap.Invoke((Action)(() => DrawPosition(point)));
        }

        private void DrawPosition(TrackPoint point)
        {
            var gamePoint = TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude);

            if (_previousPoint != null)
            {
                var image = pictureBoxMap.Image;

                using (var graphics = Graphics.FromImage(image))
                {
                    DrawSegmentLine(_overallOffsets, graphics, gamePoint, _previousPoint, Pens.Green);
                }

                pictureBoxMap.Image = image;
            }

            _previousPoint = gamePoint;
        }

        private void UpdateTurnCommands(List<TurnDirection> commands)
        {
            var text = string.Join(
                ", ",
                commands.Select(t =>t.ToString()));

            textBoxAvailableCommands.Invoke((Action)(() => textBoxAvailableCommands.Text = text));
        }

        private void UpdateDirection(SegmentDirection direction)
        {
            textBoxCurrentDirection.Invoke((Action)(() => textBoxCurrentDirection.Text = direction.ToString()));
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
    }
}
