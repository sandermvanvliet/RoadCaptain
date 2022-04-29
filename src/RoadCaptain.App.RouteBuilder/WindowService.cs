using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.Views;

namespace RoadCaptain.App.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext) : base(componentContext)
        {
        }

        //public void ShowMainWindow()
        //{
        //    if (CurrentWindow is MainWindow)
        //    {
        //        Activate(CurrentWindow);
        //    }
        //    else
        //    {
        //        var window = Resolve<MainWindow>();

        //        if (CurrentWindow != null)
        //        {
        //            Close(CurrentWindow);
        //        }

        //        Show(window);
        //    }
        //}

        public string? ShowSaveFileDialog(string? previousLocation)
        {
            var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrEmpty(previousLocation) && Directory.Exists(previousLocation))
            {
                initialDirectory = previousLocation;
            }

            var dialog = new SaveFileDialog
            {
                Directory = initialDirectory,
                DefaultExtension = "*.json",
                Filters = new List<FileDialogFilter>
                {
                    new() { Extensions = new List<string>{"*.json"}, Name = "RoadCaptain route file (.json)"},
                    new() { Extensions = new List<string>{"*.gpx"}, Name = "GPS Exchange Format (.gpx)"}
                },
                Title = "Save RoadCaptain route file",
            };

            return dialog.ShowAsync(CurrentWindow).GetAwaiter().GetResult();
        }

        //public bool ShowDefaultSportSelectionDialog(SportType sport)
        //{
        //    var result = MessageBox.Show(
        //        CurrentWindow,
        //        $"Do you want to use {sport} as your default selection?\nIf you do you won't have to select it again when you build more routes.",
        //        "Select default sport",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Question);

        //    return result == MessageBoxResult.Yes;
        //}

        //public MessageBoxResult ShowSaveRouteDialog()
        //{
        //    return MessageBox.Show(
        //        "Do you want to save the current route?",
        //        "Current route was changed",
        //        MessageBoxButton.YesNoCancel,
        //        MessageBoxImage.Information);
        //}

        //public MessageBoxResult ShowClearRouteDialog()
        //{
        //    return MessageBox.Show(
        //        "This action will remove all segments from the current route. Are you sure?",
        //        "Clear route",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Question);
        //}
        public void ShowErrorDialog(string message, Window owner = null)
        {
            throw new NotImplementedException();
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            InitializeApplicationLifetime(applicationLifetime);

            var desktopMainWindow = Resolve<MainWindow>();

            base.Show(desktopMainWindow);
        }

        public bool ShowDefaultSportSelectionDialog(SportType sport)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult ShowSaveRouteDialog()
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult ShowClearRouteDialog()
        {
            throw new NotImplementedException();
        }
    }
}