using System;
using Autofac;
using Microsoft.Win32;
using RoadCaptain.UserInterface.Shared;

namespace RoadCaptain.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext) : base(componentContext)
        {
        }

        public void ShowMainWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                Activate(CurrentWindow);
            }
            else
            {
                var window = Resolve<MainWindow>();

                if (CurrentWindow != null)
                {
                    Close(CurrentWindow);
                }

                Show(window);
            }
        }

        public string ShowSaveFileDialog()
        {
            var dialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "JSON files (.json)|*.json|GPS Exchange Format (.gpx)|*.gpx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            var result = ShowDialog(dialog).GetValueOrDefault();

            if (!result)
            {
                return null;
            }

            return dialog.FileName;
        }
    }
}