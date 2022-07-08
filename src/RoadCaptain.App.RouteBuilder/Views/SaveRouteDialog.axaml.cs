using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class SaveRouteDialog : Window
    {
        public SaveRouteDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StyledElement_OnInitialized(object? sender, EventArgs e)
        {
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            if (DataContext is SaveRouteDialogViewModel viewModel)
            {
                viewModel.ShouldClose += (_, _) =>
                {
                    if (!Dispatcher.UIThread.CheckAccess())
                    {
                        Dispatcher.UIThread.InvokeAsync(Close);
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }
    }
}
