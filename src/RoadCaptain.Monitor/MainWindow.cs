using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RoadCaptain.Ports;

namespace RoadCaptain.Monitor
{
    public partial class MainWindow : Form
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ISegmentStore _segmentStore;
        private List<Segment> _segments;

        public MainWindow(
            MonitoringEvents monitoringEvents,
            ISegmentStore segmentStore)
        {
            _monitoringEvents = monitoringEvents;
            _segmentStore = segmentStore;

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
        }

        private void DrawSegments()
        {
            var image = new Bitmap(pictureBoxMap.Width, pictureBoxMap.Height);
            var graphics = Graphics.FromImage(image);

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

            var overallOffsets = new Offsets(
                pictureBoxMap.Width,
                segmentsWithOffsets.SelectMany(s => s.GameCoordinates).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                DrawSegment(overallOffsets, segment.GameCoordinates, graphics);
            }

            pictureBoxMap.Image = image;
        }

        private void DrawSegment(Offsets offsets, List<TrackPoint> data, Graphics graphics)
        {
            for (var index = 1; index < data.Count; index++)
            {
                var previousPoint = data[index-1];
                var point = data[index];
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
                    graphics.DrawLine(Pens.Red, (int)previousScaledX, (int)previousScaledY, (int)scaledX, (int)scaledY);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Debugger.Break();
                }
            }
        }
    }

    public class Offsets
    {
        public Offsets(float imageWidth, List<TrackPoint> data)
        {
            ImageWidth = imageWidth;

            MinX = (float)data.Min(p => p.Latitude);
            MaxX = (float)data.Max(p => p.Latitude);
                   
            MinY = (float)data.Min(p => p.Longitude);
            MaxY = (float)data.Max(p => p.Longitude);
        }

        public float ImageWidth { get; }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinY { get; }
        public float MaxY { get; }
        public float RangeX => MaxX - MinX;
        public float RangeY => MaxY - MinY;

        // If minX is negative the offset is positive because we shift everything to the right, if it is positive the offset is negative beause we shift to the left
        public float OffsetX => -MinX;
        public float OffsetY => -MinY;

        public float ScaleFactor
        {
            get
            {
                if (RangeY > RangeX)
                {
                    return (ImageWidth - 1) / RangeY;
                }

                return (ImageWidth - 1) / RangeX;
            }
        }
    }
}
