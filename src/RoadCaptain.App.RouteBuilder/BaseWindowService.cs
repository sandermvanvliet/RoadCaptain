using System;
using System.IO;
using Autofac;
using Avalonia.Controls;

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

        public string ShowOpenFileDialog(string previousLocation)
        {
            //var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            //if (!string.IsNullOrEmpty(previousLocation) && Directory.Exists(previousLocation))
            //{
            //    initialDirectory = previousLocation;
            //}

            //var dialog = new OpenFileDialog
            //{
            //    AddExtension = true,
            //    DefaultExt = ".json",
            //    Filter = "JSON files (.json)|*.json",
            //    Multiselect = false,
            //    InitialDirectory = initialDirectory
            //};

            //var result = ShowDialog(dialog).GetValueOrDefault();

            //return result
            //    ? dialog.FileName
            //    : null;

            throw new NotImplementedException();
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
            //if (owner != null)
            //{
            //    MessageBox.Show(
            //        owner,
            //        message,
            //        "An error occurred",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
            //else if (CurrentWindow != null)
            //{
            //    MessageBox.Show(
            //        CurrentWindow,
            //        message,
            //        "An error occurred",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
            //else
            //{
            //    MessageBox.Show(
            //        message,
            //        "An error occurred",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
            
            throw new NotImplementedException();
        }

        protected virtual TType Resolve<TType>()
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