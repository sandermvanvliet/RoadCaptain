using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RoadCaptain.RouteBuilder.ViewModels;

namespace RoadCaptain.RouteBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel
            {
                Route = new RouteViewModel
                {
                    Sequence = new List<SegmentSequenceViewModel>
                    {
                        new()
                        {
                            Ascent = 10,
                            Descent = 0,
                            Distance = 1.25,
                            Segment = "abcd",
                            SequenceNumber = 1,
                            TurnImage = "Assets/turnleft.jpg"
                        },
                        new()
                        {
                            Ascent = 35,
                            Descent = 78,
                            Distance = 2.75,
                            Segment = "efgh",
                            SequenceNumber = 2,
                            TurnImage = "Assets/turnright.jpg"
                        },
                        new()
                        {
                            Ascent = 0,
                            Descent = 50,
                            Distance = 10.5,
                            Segment = "ijkl",
                            SequenceNumber = 1,
                            TurnImage = "Assets/gostraight.jpg"
                        }
                    }
                }
            };
            InitializeComponent();
        }
    }
}
