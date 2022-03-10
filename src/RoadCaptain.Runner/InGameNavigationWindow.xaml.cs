using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.Runner.ViewModels;
using Point = System.Drawing.Point;

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

        private void Window_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void InGameNavigationWindow_OnInitialized(object? sender, EventArgs e)
        {
            if (AppSettings.Default.InGameWindowLocation != Point.Empty)
            {
                Left = AppSettings.Default.InGameWindowLocation.X;
                Top = AppSettings.Default.InGameWindowLocation.Y;
            }
        }

        private void InGameNavigationWindow_OnLocationChanged(object sender, EventArgs e)
        {
            AppSettings.Default.InGameWindowLocation = new Point((int)Left, (int)Top);
            AppSettings.Default.Save();
        }
    }
}