// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        MessageBoxResult ShowClearRouteDialog();
    }
}
