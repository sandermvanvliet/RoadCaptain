// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class OpenRouteDialog : DialogWindow
    {
        public OpenRouteDialog()
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;
            
            if (DataContext is OpenRouteDialogViewModel viewModel)
            {
                viewModel.ShouldClose += (_, _) =>
                {
                    if (!Dispatcher.UIThread.CheckAccess())
                    {
                        Dispatcher.UIThread.InvokeAsync(Close);
                    }
                    else
                    {
                        DialogResult = viewModel.SelectedRoute != null || !string.IsNullOrEmpty(viewModel.RouteFilePath)
                            ? DialogResult.Ok
                            : DialogResult.Cancel;
                        
                        Close();
                    }
                };
            }
        }
    }
}
