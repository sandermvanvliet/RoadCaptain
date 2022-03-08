using System.Windows;
using RoadCaptain.Adapters;
using RoadCaptain.RouteBuilder.ViewModels;

namespace RoadCaptain.RouteBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var viewModel = new MainWindowViewModel(new RouteStoreToDisk(), new SegmentStore());

            var mainWindow = new MainWindow(viewModel);

            mainWindow.Show();
        }
    }
}
