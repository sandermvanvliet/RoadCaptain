using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for InGameNavigationWindow.xaml
    /// </summary>
    public partial class InGameNavigationWindow : Window
    {
        private readonly InGameNavigationWindowViewModel _windowViewModel;

        public InGameNavigationWindow(InGameNavigationWindowViewModel windowViewModel)
        {
            _windowViewModel = windowViewModel;
            DataContext = windowViewModel;

            _windowViewModel.PropertyChanged += WindowViewModelPropertyChanged;

            InitializeComponent();
        }

        private void WindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}