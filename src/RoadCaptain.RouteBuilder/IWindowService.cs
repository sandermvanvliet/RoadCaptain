using System.Windows;

namespace RoadCaptain.RouteBuilder
{
    public interface IWindowService
    {
        string ShowOpenFileDialog();
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
        void ShowNewVersionDialog(Release release);
        string ShowSaveFileDialog();
        bool ShowDefaultSportSelectionDialog(SportType sport);
        MessageBoxResult ShowSaveRouteDialog();
    }
}