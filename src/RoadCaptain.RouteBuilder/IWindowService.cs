using System.Windows;

namespace RoadCaptain.RouteBuilder
{
    public interface IWindowService
    {
        string ShowOpenFileDialog(string previousLocation);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
        void ShowNewVersionDialog(Release release);
        string ShowSaveFileDialog(string previousLocation);
        bool ShowDefaultSportSelectionDialog(SportType sport);
        MessageBoxResult ShowSaveRouteDialog();
    }
}