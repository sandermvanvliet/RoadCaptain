using System;

namespace RoadCaptain.RouteBuilder.ViewModels
{
    public class SegmentSequenceViewModel
    {
        public SegmentSequenceViewModel()
        {
            Segment = "some segment id";
            TurnImage = ImageFromTurn(TurnDirection.Left);
            Distance = 2.34;
            Ascent = 100;
            Descent = 50;
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

        public int SequenceNumber { get; set; }
        public string TurnImage { get; set; }
        public string Segment { get; set; }
        public double Distance { get; set; }
        public double Descent { get; set; }
        public double Ascent { get; set; }
    }
}