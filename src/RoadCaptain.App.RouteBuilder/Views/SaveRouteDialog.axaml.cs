// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

                Dispatcher.UIThread.InvokeAsync(() => viewModel.Initialize());
            }
        }

        private void RoutesListBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // This prevents the situation where the PointerPressed event bubbles
            // up to the window and initiates the window drag operation.
            // It fixes a bug where the combo box can't be opened.
            e.Handled = true;
        }
    }
}

