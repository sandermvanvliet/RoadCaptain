using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.RouteBuilder
{
    public abstract class BaseWindowService
    {
        private readonly IComponentContext _componentContext;

        protected BaseWindowService(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        protected Window? CurrentWindow { get; private set; }

        public async Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrEmpty(previousLocation) && Directory.Exists(previousLocation))
            {
                initialDirectory = previousLocation;
            }

            var dialog = new OpenFileDialog
            {
                Directory = initialDirectory,
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new() { Extensions = new List<string>{"json"}, Name = "RoadCaptain route file (.json)"}
                },
                Title = "Open RoadCaptain route file"
            };

            var selectedFiles = await dialog.ShowAsync(CurrentWindow);

            if (selectedFiles != null && selectedFiles.Any())
            {
                return selectedFiles.First();
            }

            return null;
        }

        public void ShowNewVersionDialog(Release release)
        {
            throw new NotImplementedException();
            //var window = Resolve<UpdateAvailableWindow>();

            //window.DataContext = new UpdateAvailableViewModel(release);

            //ShowDialog(window);
        }

        public virtual void ShowErrorDialog(string message, Window owner)
        {
            MessageBox.ShowAsync(
                message,
                "An error occurred",
                MessageBoxButton.Ok,
                owner,
                MessageBoxIcon.Error)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual TType Resolve<TType>() where TType : notnull
        {
            return _componentContext.Resolve<TType>();
        }

        protected virtual bool? ShowDialog(Window window)
        {
            if (window.Owner == null)
            {
                //window.Owner = CurrentWindow;
            }

            window.ShowDialog(CurrentWindow).GetAwaiter().GetResult();

            return true;
        }

        protected virtual void Show(Window window)
        {
            CurrentWindow = window;

            window.Show();
        }

        protected virtual void Close(Window window)
        {
            window.Close();
            CurrentWindow = null;
        }

        protected virtual bool Activate(Window window)
        {
            window.Activate();
            return true;
        }

        //protected virtual bool? ShowDialog(CommonDialog dialog)
        //{
        //    return dialog.ShowDialog(CurrentWindow);
        //}
    }
}