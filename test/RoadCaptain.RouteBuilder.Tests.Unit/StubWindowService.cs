﻿using System.Windows;

namespace RoadCaptain.RouteBuilder.Tests.Unit
{
    public class StubWindowService : IWindowService
    {
        public string ShowOpenFileDialog()
        {
            throw new System.NotImplementedException();
        }

        public void ShowErrorDialog(string message, Window owner = null)
        {
            throw new System.NotImplementedException();
        }

        public void ShowMainWindow()
        {
            throw new System.NotImplementedException();
        }

        public void ShowNewVersionDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public string ShowSaveFileDialog()
        {
            throw new System.NotImplementedException();
        }

        public bool ShowDefaultSportSelectionDialog(SportType sport)
        {
            return false;
        }
    }
}