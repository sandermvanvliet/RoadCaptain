﻿using System.ComponentModel;
using System.Windows;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _windowViewModel;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            _windowViewModel = mainWindowViewModel;
            DataContext = mainWindowViewModel;

            _windowViewModel.PropertyChanged += WindowViewModelPropertyChanged;

            InitializeComponent();
        }

        private void WindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            
        }
    }
}