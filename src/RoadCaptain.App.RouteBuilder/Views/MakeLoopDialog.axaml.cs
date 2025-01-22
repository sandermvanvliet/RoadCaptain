// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MakeLoopDialog : DialogWindow
    {
        public MakeLoopDialog()
        {
            InitializeComponent();
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;
            
            if (DataContext is MakeLoopDialogViewModel viewModel)
            {
                viewModel.ShouldClose += (_, _) =>
                {
                    if (!Dispatcher.UIThread.CheckAccess())
                    {
                        Dispatcher.UIThread.InvokeAsync(Close);
                    }
                    else
                    {
                        DialogResult = viewModel.DialogResult;
                        Close();
                    }
                };
            }
        }
    }
}

