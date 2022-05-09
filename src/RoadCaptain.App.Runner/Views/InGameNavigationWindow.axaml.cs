using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace RoadCaptain.App.Runner.Views
{
    public partial class InGameNavigationWindow : Window
    {
        public InGameNavigationWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
        }

        private void InGameNavigationWindow_OnActivated(object? sender, EventArgs e)
        {
        }

        private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e)
        {
        }
    }
}
