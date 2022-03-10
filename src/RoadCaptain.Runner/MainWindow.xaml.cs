using System.Windows;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
